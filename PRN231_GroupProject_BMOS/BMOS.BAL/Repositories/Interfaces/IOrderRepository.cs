using BMOS.BAL.DTOs.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Validators;

namespace BMOS.BAL.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        public Task<GetOrderResponse> CheckOutAsync(PostOrderRequest request);
        public Task ConfirmOrderToProcessingAsync(int orderId);
        public Task ConfirmOrderToDoneAsync(int orderId);
        public Task CancelOrderAsync(int customerId, int orderId);
        public Task<List<GetOrderResponse>> GetOrdersAsync();
        public Task<GetOrderResponse> GetOrderDetailAsync(int id);
        public Task<List<GetOrderResponse>> GetCustomerOrders(int customerId);
        public Task<GetOrderResponse> GetCustomerOrder(int customerId, int orderId);
    }
}
