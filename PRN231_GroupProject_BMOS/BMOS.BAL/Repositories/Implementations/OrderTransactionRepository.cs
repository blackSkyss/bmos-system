using AutoMapper;
using BMOS.BAL.DTOs.OrderDetails;
using BMOS.BAL.DTOs.OrderTransactions;
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
    public class OrderTransactionRepository : IOrderTransactionRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public OrderTransactionRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        #region Get order transactions
        public async Task<IEnumerable<GetOrderLogResponse>> GetOrderTransactionsAsync()
        {
            try
            {
                var orderTransactions = await _unitOfWork.OrderTransactionDAO.GetOrderTransactionsAsync();
                return _mapper.Map<IEnumerable<GetOrderLogResponse>>(orderTransactions);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
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

        #region Get order transactions by ordeId
        public async Task<IEnumerable<GetOrderLogResponse>> GetOrderTransactionsByOrderIdAsync(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    throw new BadRequestException("Id is invalid!");
                }

                var orderTransactions = await _unitOfWork.OrderTransactionDAO.GetOrderTransactionsByOrderIdAsync((int)id);
                if(orderTransactions == null || orderTransactions.Count() == 0) {
                    throw new NotFoundException("Order does not exist!");
                }

                return _mapper.Map<IEnumerable<GetOrderLogResponse>>(orderTransactions);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
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
    }
}
