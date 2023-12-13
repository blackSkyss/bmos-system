using AutoMapper;
using BMOS.BAL.DTOs.Accounts;
using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public CustomerRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        #region GetCustomerByAccountIdAsync
        public async Task<GetCustomerResponse> GetCustomerByAccountIdAsync(int accountId)
        {
            try
            {
                Customer customer = await _unitOfWork.CustomerDAO.GetCustomerByAccountIdAsync(accountId);
                if (customer == null)
                {
                    throw new NotFoundException("AccountId does not exist in system");
                }
                return _mapper.Map<GetCustomerResponse>(customer);
            }
            catch(NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }

        }

        #endregion

        #region UpdateCustomerProfileByAccountIdAsync
        public async Task<GetCustomerResponse> UpdateCustomerProfileByAccountIdAsync(int accountId,
                                                                                     FireBaseImage fireBaseImage,
                                                                                     UpdateCustomerRequest updateCustomerRequest)
        {
            try
            {
                Customer customer = await _unitOfWork.CustomerDAO.GetCustomerByAccountIdAsync(accountId);

                if (customer == null)
                {
                    throw new NotFoundException("AccountId does not exist in system");
                }

                customer.Address = updateCustomerRequest.Address;
                customer.FullName = updateCustomerRequest.FullName;
                customer.BirthDate = updateCustomerRequest.BirthDate;
                customer.Gender = updateCustomerRequest.Gender;
                if(updateCustomerRequest.PasswordHash != null)
                {
                    customer.Account.PasswordHash = StringHelper.EncryptData(updateCustomerRequest.PasswordHash);
                }

                #region Upload image to firebase
                if (updateCustomerRequest.Avatar != null)
                {
                    FileHelper.SetCredentials(fireBaseImage);
                    await FileHelper.DeleteImageAsync(customer.AvatarID, "Customer");
                    FileStream fileStream = FileHelper.ConvertFormFileToStream(updateCustomerRequest.Avatar);
                    Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Customer");
                    customer.Avatar = result.Item1;
                    customer.AvatarID = result.Item2;
                }
                #endregion

                _unitOfWork.CustomerDAO.UpdateCustomerProfile(customer);
                await this._unitOfWork.CommitAsync();
                return _mapper.Map<GetCustomerResponse>(customer);
            }

            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString("AccountId", ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }

        }
        #endregion

        //get all customer
        public async Task<List<GetCustomerResponse>> GetCustomersAsync()
        {
            try
            {
                List<Customer> customers = await _unitOfWork.CustomerDAO.GetCustomersAsync();
                return _mapper.Map<List<GetCustomerResponse>>(customers);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }

        //ban customer
        public async Task<GetCustomerResponse> BanCustomerAsync(int accountId)
        {
            try
            {
                Customer customer = await _unitOfWork.CustomerDAO.GetCustomerByAccountIdAsync(accountId);
                if (customer == null)
                {
                    throw new NotFoundException("AccountId does not exist in system.");
                }
                customer.Account.Status = Convert.ToBoolean((int)AccountEnum.Status.INACTIVE);
                _unitOfWork.CustomerDAO.BanCustomer(customer);
                await this._unitOfWork.CommitAsync();
                return _mapper.Map<GetCustomerResponse>(customer);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #region Register
        public async Task<GetCustomerResponse> Register(FireBaseImage fireBaseImage, RegisterRequest registerRequest)
        {
            try
            {
                var role = await _unitOfWork.RoleDAO.GetRoleAsync((int)RoleEnum.Role.CUSTOMER);
                var customerByEmail = await _unitOfWork.CustomerDAO.GetCustomerByEmailAsync(registerRequest.Email);
                if (customerByEmail != null)
                {
                    throw new BadRequestException("Email already exist in the system.");
                }

                var customerPhone = await _unitOfWork.CustomerDAO.GetCustomerByPhoneAsync(registerRequest.Phone);
                if (customerPhone != null)
                {
                    throw new BadRequestException("Phone already exist in the system.");
                }

                // assign registerRequest to account
                Account account = new Account
                {
                    Email = registerRequest.Email,
                    PasswordHash = StringHelper.EncryptData(registerRequest.PasswordHash),
                    Role = role,
                    Status = Convert.ToBoolean(AccountEnum.Status.ACTIVE)
                };
                await _unitOfWork.AccountDAO.AddNewAccount(account);

                // assign registerRequest to customer
                Customer customer = new Customer
                {
                    FullName = registerRequest.FullName,
                    Address = registerRequest.Address,
                    BirthDate = registerRequest.BirthDate,
                    Gender = registerRequest.Gender,
                    Phone = registerRequest.Phone,
                    Account = account,
                };

                // Upload image to firebase
                FileHelper.SetCredentials(fireBaseImage);
                FileStream fileStream = FileHelper.ConvertFormFileToStream(registerRequest.Avatar);
                Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Customer");
                customer.Avatar = result.Item1;
                customer.AvatarID = result.Item2;

                Wallet wallet = new Wallet()
                {
                    Account = account,
                    Balance = 0
                };

                await _unitOfWork.CustomerDAO.AddNewCustomer(customer);
                await this._unitOfWork.WalletDAO.CreateWalletAsync(wallet);

                //Save to Database
                await _unitOfWork.CommitAsync();

                return new GetCustomerResponse
                {
                    Phone = customer.Phone,
                    AccountID = customer.AccountID,
                    Address = customer.Address,
                    Avatar = customer.Avatar,
                    BirthDate = customer.BirthDate,
                    FullName = customer.FullName,
                    Gender = customer.Gender,
                    Account = _mapper.Map<GetAccountResponse>(customer.Account),
                };
            }
            catch (BadRequestException ex)
            {
                string fieldNameError = "";
                if (ex.Message.ToLower().Contains("email"))
                {
                    fieldNameError = "Email";
                } else if (ex.Message.ToLower().Contains("phone"))
                {
                    fieldNameError = "Phone";
                }
                string error = ErrorHelper.GetErrorString(fieldNameError, ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion
    }
}
