using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.Accounts;
using BMOS.BAL.DTOs.Authentications;
using BMOS.BAL.DTOs.JWT;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;

namespace BMOS.WebAPI.Controllers
{
    public class AuthenticationsController : ODataController
    {
        private IAuthenticationRepository _authenticationRepository;
        private IValidator<PostAccountRequest> _loginValidator;
        private IValidator<PostRecreateTokenRequest> _postRecreateTokenValidator;
        private IOptions<JwtAuth> _jwtAuthOptions;
        public AuthenticationsController(IAuthenticationRepository authenticationRepository,
                                         IValidator<PostAccountRequest> loginValidator,
                                         IOptions<JwtAuth> jwtAuthOptions,
                                         IValidator<PostRecreateTokenRequest> postRecreateTokenValidator)
        {
            _authenticationRepository = authenticationRepository;
            _loginValidator = loginValidator;
            _jwtAuthOptions = jwtAuthOptions;
            _postRecreateTokenValidator = postRecreateTokenValidator;
        }

        #region Login
        [EnableQuery]
        [HttpPost("odata/authentications/login")]
        public async Task<IActionResult> Login([FromBody] PostAccountRequest request)
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }

            var result = await _authenticationRepository.LoginAsync(request, _jwtAuthOptions.Value);
            return Ok(result);
        }
        #endregion

        #region Recreate token
        [EnableQuery]
        [HttpPost("odata/authentications/recreate-token")]
        public async Task<IActionResult> RecreateToken([FromBody] PostRecreateTokenRequest request)
        {
            var validationResult = await _postRecreateTokenValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }

            var result = await _authenticationRepository.ReCreateTokenAsync(request, _jwtAuthOptions.Value);
            return Ok(result);
        }
        #endregion
    }
}
