using OrderManagement.DTOsModels;
using OrderManagement.Filters;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Linq;
using System.Web.Http;

namespace OrderManagement.Controllers
{
    [AuthorizeSession]
    [RoutePrefix("api/order")]
    public class OrderController : BaseApiController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            var dbContext = new OrderManagementEntities();
            IOrderRepository orderRepository = new OrderRepository(dbContext);
            _orderService = orderService;
        }

        [HttpPost()]
        [Route("place")]
        public IHttpActionResult PlaceOrder([FromBody] PlaceOrderRequest request)
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

        [HttpGet()]
        [Route("history/{userId}")]
        public IHttpActionResult GetOrderHistory(int userId)
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
