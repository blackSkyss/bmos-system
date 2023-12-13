using AutoMapper;
using BMOS.BAL.DTOs.Wallets;
using BMOS.BAL.DTOs.Wallets;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class WalletRepository: IWalletRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public WalletRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        public  async Task<GetWalletsResponse> GetWalletByUserID(int userID)
        {
            try
            {
                Wallet wallet = await this._unitOfWork.WalletDAO.GetWalletByUserID(userID);
                if (wallet == null)
                {
                    throw new NotFoundException("CustomerId does not found in the system.");
                }
                return this._mapper.Map<GetWalletsResponse>(wallet);
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
    }
}
