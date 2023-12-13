using AutoMapper;
using BMOS.BAL.DTOs.OrderDetails;
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
    public class OrderDetailRepository: IOrderDetailRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public OrderDetailRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<GetOrderDetailResponse>> GetOrderDetailsByOrderID(int orderID)
        {
            try
            {
                List<OrderDetail> orderDetails = await this._unitOfWork.OrderDetailDAO.GetOrderDetailsByOrderID(orderID);
                if(orderDetails == null)
                {
                    throw new NotFoundException("Order details not found");
                }
                return this._mapper.Map<List<GetOrderDetailResponse>>(orderDetails);
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

        #region Get order detail by order id
        public async Task<IEnumerable<GetOrderDetailResponse>> GetOrderDetailsByOrderIdAsync(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    throw new BadRequestException("Id is invalid!");
                }

                var orderDetails = await _unitOfWork.OrderDetailDAO.GetOrderDetailByOrderIdAsync((int)id);
                if(orderDetails == null || orderDetails.Count() == 0)
                {
                    throw new NotFoundException("Order does not exist!");
                }

                return _mapper.Map<IEnumerable<GetOrderDetailResponse>>(orderDetails);
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
