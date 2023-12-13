using BMOS.BAL.Authorization;
using BMOS.BAL.DTOs.Orders;
using BMOS.BAL.Errors;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.BAL.Validators.Orders;
using BMOS.DAL.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

namespace BMOS.WebAPI.Controllers
{

    public class OrdersController : ODataController
    {
        private IOrderRepository _orderRepository;
        private IValidator<PostOrderRequest> _postOrderValidator;
        public OrdersController(IOrderRepository orderRepository, IValidator<PostOrderRequest> postOrderValidator)
        {
            _orderRepository = orderRepository;
            _postOrderValidator = postOrderValidator;
        }

        #region Check out
        [EnableQuery]
        [PermissionAuthorize("Customer")]
        public async Task<IActionResult> Post([FromBody] PostOrderRequest request)
        {
            var validationResult = await _postOrderValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                string error = ErrorHelper.GetErrorsString(validationResult);
                throw new BadRequestException(error);
            }

            GetOrderResponse order = await _orderRepository.CheckOutAsync(request);
            return Created(order);
        }
        #endregion

        #region Confirm order to processing
        [HttpPut("odata/orders/update-processing/{orderId}")]
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> ConfirmOrderToProcessing([FromRoute] int orderId)
        {
            await _orderRepository.ConfirmOrderToProcessingAsync(orderId);
            return NoContent();
        }
        #endregion

        #region Confirm order to done
        [HttpPut("odata/orders/update-done/{orderId}")]
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> ConfirmOrderToDone([FromRoute] int orderId)
        {
            await _orderRepository.ConfirmOrderToDoneAsync(orderId);
            return NoContent();
        }
        #endregion

        #region Cancel order
        [HttpDelete("odata/orders/order({orderId})/customer({customerId})/cancel")]
        [EnableQuery]
        [PermissionAuthorize("Customer", "Staff")]
        public async Task<IActionResult> Delete([FromRoute] int orderId, [FromRoute] int customerId)
        {
            await _orderRepository.CancelOrderAsync(customerId, orderId);
            return NoContent();
        }
        #endregion

        [HttpGet("odata/orders/customer/{customerId}")]
        [EnableQuery]
        [PermissionAuthorize("Customer")]
        public async Task<IActionResult> CustomerOrders([FromRoute] int customerId)
        {
            List<GetOrderResponse> orders = await this._orderRepository.GetCustomerOrders(customerId);
            return Ok(orders);
        }

        [HttpGet("odata/orders/order({orderId})/customer({customerId})")]
        [EnableQuery]
        [PermissionAuthorize("Customer")]
        public async Task<IActionResult> CustomerOrder([FromRoute] int orderId, [FromRoute] int customerId)
        {
            GetOrderResponse order = await this._orderRepository.GetCustomerOrder(customerId, orderId);
            return Ok(order);
        }

        #region Get orders
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Get()
        {
            var result = await _orderRepository.GetOrdersAsync();
            return Ok(result);
        }
        #endregion

        #region Get order detail
        [EnableQuery]
        [PermissionAuthorize("Staff")]
        public async Task<IActionResult> Get([FromRoute] int key)
        {
            var result = await _orderRepository.GetOrderDetailAsync(key);
            return Ok(result);
        }
        #endregion
    }
}
