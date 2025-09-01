using OrderManagement.DTOsModels;
using OrderManagement.Services.Contracts;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace OrderManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/order")]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("place")]
        public IActionResult PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            try
            {
                if (request == null)
                    return Fail<object>("Order request is missing.");

                if (!ModelState.IsValid)
                    return Fail<object>("Invalid order data.");

                _orderService.PlaceOrder(request);

                CommonUtils.CommonUtils.LogMessage($"Order placed successfully for customer: {request.UserId}");

                return Success<object>(null, "Order placed successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error placing order for customer {request?.UserId}: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred during order placement.");
            }
        }

        [HttpGet("history/{userId}")]
        public IActionResult GetOrderHistory(int userId)
        {
            try
            {
                if (userId <= 0)
                    return Fail<object>("Invalid user ID.");

                var history = _orderService.GetOrderHistory(userId);

                if (history == null || !history.Any())
                    return Fail<object>("Invalid user ID.");

                return Success(history, "Order history retrieved successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error retrieving order history for userId={userId}: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred while retrieving order history.");
            }
        }
    }
}
