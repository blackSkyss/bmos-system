using AutoMapper;
using BMOS.BAL.DTOs.Accounts;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public AccountRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this._unitOfWork = (UnitOfWork)unitOfWork;
            this._mapper = mapper;
        }

        public async Task<GetAccountResponse> ChangPassword(int accountId, ChangePasswordRequest changPasswordRequest)
        {
            try
            {
                var account = await _unitOfWork.AccountDAO.GetAccountById(accountId);
                if (account == null)
                {
                    throw new NotFoundException("AccountId does not exist in system.");
                }

                if (StringHelper.EncryptData(changPasswordRequest.OldPassWord) != account.PasswordHash)
                {
                    throw new BadRequestException("Old password does not match with current password.");
                }

                if (changPasswordRequest.NewPassWord != changPasswordRequest.ConFirmPassword)
                {
                    throw new BadRequestException("New password and old password do not match each other.");
                }
                account.PasswordHash = StringHelper.EncryptData(changPasswordRequest.NewPassWord);
                _unitOfWork.AccountDAO.ChangePassword(account);
                _unitOfWork.Commit();
                return _mapper.Map<GetAccountResponse>(account);
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

    }
}
