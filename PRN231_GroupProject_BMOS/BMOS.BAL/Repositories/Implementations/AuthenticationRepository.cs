using AutoMapper;
using BMOS.BAL.DTOs.Accounts;
using BMOS.BAL.DTOs.Authentications;
using BMOS.BAL.DTOs.JWT;
using BMOS.BAL.DTOs.Orders;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public AuthenticationRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        #region Login
        public async Task<PostLoginResponse> LoginAsync(PostAccountRequest request, JwtAuth jwtAuth)
        {
            try
            {
                var account = await _unitOfWork.AccountDAO.GetAccountByEmailAndPasswordAsync(request.Email, StringHelper.EncryptData(request.PasswordHash.Trim()));
                if (account == null)
                {
                    throw new BadRequestException("Email or password is invalid.");
                }

                var loginResponse = new PostLoginResponse();
                loginResponse.AccountId = account.ID;
                loginResponse.Email = account.Email;
                loginResponse.Role = account.Role.Name;

                if (account.Role.ID == (int)RoleEnum.Role.CUSTOMER)
                {
                    var customer = await _unitOfWork.CustomerDAO.GetCustomerByIdAsync(account.ID);
                    loginResponse.FullName = customer.FullName;
                }
                else if (account.Role.ID == (int)RoleEnum.Role.STAFF)
                {
                    var staff = await _unitOfWork.StaffDAO.GetStaffDetailAsync(account.ID);
                    loginResponse.FullName = staff.FullName;
                }
                else
                {
                    loginResponse.FullName = account.Email;
                }

                var resultLogin = await GenerateToken(loginResponse, jwtAuth, account);
                return resultLogin;
            }
            catch (BadRequestException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Recreate token
        public async Task<PostRecreateTokenResponse> ReCreateTokenAsync(PostRecreateTokenRequest request, JwtAuth jwtAuth)
        {
            #region Config
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var secretKeyBytes = Encoding.UTF8.GetBytes(jwtAuth.Key);
            var tokenValidationParameters = new TokenValidationParameters
            {
                //Tự cấp token nên phần này bỏ qua
                ValidateIssuer = false,
                ValidateAudience = false,
                //Ký vào token
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
                ValidateLifetime = false, //khong kiem tra token het han
                ClockSkew = TimeSpan.Zero // thoi gian expired dung voi thoi gian chi dinh
            };
            #endregion

            try
            {
                #region Validation
                //Check 1: Access token is valid format
                var tokenVerification = jwtTokenHandler.ValidateToken(request.AccessToken, tokenValidationParameters, out var validatedToken);

                //Check 2: Check Alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (result == false)
                    {
                        throw new BadRequestException("Invalid token.");
                    }
                }

                //Check 3: check accessToken expried?
                var utcExpiredDate = long.Parse(tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expiredDate = DateHelper.ConvertUnixTimeToDateTime(utcExpiredDate);
                if (expiredDate > DateTime.UtcNow)
                {
                    throw new BadRequestException("Access token has not yet expired.");
                }

                //Check 4: Check refresh token exist in Db
                Token existedRefreshToken = await this._unitOfWork.TokenDAO.GetTokenByRefreshTokenAsync(request.RefreshToken);
                if (existedRefreshToken == null)
                {
                    throw new NotFoundException("Refresh token does not exist.");
                }

                //Check 5: Refresh Token is used / revoked?
                if (existedRefreshToken.IsUsed)
                {
                    throw new BadRequestException("Refresh token is used.");
                }
                if (existedRefreshToken.IsRevoked)
                {
                    throw new BadRequestException("Refresh token is revoked.");
                }

                //Check 6: Id of refresh token == id of access token
                var jwtId = tokenVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (existedRefreshToken.JwtID.Equals(jwtId) == false)
                {
                    throw new Exception("Refresh token is not match with access token.");
                }

                //Check 7: refresh token is expired
                if (existedRefreshToken.ExpiredDate < DateTime.UtcNow)
                {
                    throw new Exception("Refresh token expired.");
                }
                #endregion

                #region Update old refresh token in Db
                existedRefreshToken.IsRevoked = true;
                existedRefreshToken.IsUsed = true;
                this._unitOfWork.TokenDAO.UpdateToken(existedRefreshToken);
                await this._unitOfWork.CommitAsync();
                #endregion

                #region Create new token
                var loginResponse = new PostLoginResponse();
                loginResponse.AccountId = existedRefreshToken.Account.ID;
                loginResponse.Email = existedRefreshToken.Account.Email;
                loginResponse.Role = existedRefreshToken.Account.Role.Name;

                if (existedRefreshToken.Account.Role.ID == (int)RoleEnum.Role.CUSTOMER)
                {
                    var customer = await _unitOfWork.CustomerDAO.GetCustomerByIdAsync(existedRefreshToken.Account.ID);
                    loginResponse.FullName = customer.FullName;
                }
                else if (existedRefreshToken.Account.Role.ID == (int)RoleEnum.Role.STAFF)
                {
                    var staff = await _unitOfWork.StaffDAO.GetStaffDetailAsync(existedRefreshToken.Account.ID);
                    loginResponse.FullName = staff.FullName;
                }
                else
                {
                    loginResponse.FullName = "Owner Store";
                }

                var newRefreshToken = await GenerateToken(loginResponse, jwtAuth, existedRefreshToken.Account);
                #endregion

                var newToken = new PostRecreateTokenResponse
                {
                    AccessToken = newRefreshToken.AccessToken,
                    RefreshToken = newRefreshToken.RefreshToken,
                };

                return newToken;
            }
            catch (BadRequestException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Generate token
        private async Task<PostLoginResponse> GenerateToken(PostLoginResponse response, JwtAuth jwtAuth, Account account)
        {
            try
            {
                var jwtTokenHandler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuth.Key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new ClaimsIdentity(new[] {
                 new Claim(JwtRegisteredClaimNames.Sub, response.Email),
                 new Claim(JwtRegisteredClaimNames.Email, response.Email),
                 new Claim(JwtRegisteredClaimNames.Name, response.FullName),
                 new Claim("Role", response.Role),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
             });

                var tokenDescription = new SecurityTokenDescriptor
                {
                    Issuer = jwtAuth.Issuer,
                    Audience = jwtAuth.Audience,
                    Subject = claims,
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = credentials,
                };

                var token = jwtTokenHandler.CreateToken(tokenDescription);
                string accessToken = jwtTokenHandler.WriteToken(token);

                string refreshToken = GenerateRefreshToken();
                Token refreshTokenModel = new Token
                {
                    JwtID = token.Id,
                    RefreshToken = refreshToken,
                    CreatedDate = DateTime.UtcNow,
                    ExpiredDate = DateTime.UtcNow.AddDays(5),
                    IsUsed = false,
                    IsRevoked = false,
                    Account = account,
                };

                await _unitOfWork.TokenDAO.CreateTokenAsync(refreshTokenModel);
                await _unitOfWork.CommitAsync();

                response.AccessToken = accessToken;
                response.RefreshToken = refreshToken;

                return response;
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Generate refresh token
        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }
        #endregion
    }
}
