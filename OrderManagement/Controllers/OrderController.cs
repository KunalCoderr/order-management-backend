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
    public class OrderController : ApiController
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
                    return BadRequest("Order request cannot be null.");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                _orderService.PlaceOrder(request);

                CommonUtils.CommonUtils.LogMessage($"Order placed successfully for customer: {request.UserId}");

                return Ok(new { message = "Order placed successfully." });
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error placing order for customer {request?.UserId}: {ex.Message}\n{ex.StackTrace}");
                return InternalServerError();
            }
        }

        [HttpGet()]
        [Route("history/{userId}")]
        public IHttpActionResult GetOrderHistory(int userId)
        {
            try
            {
                if (userId <= 0)
                    return BadRequest("Invalid user ID.");

                var history = _orderService.GetOrderHistory(userId);

                if (history == null || !history.Any())
                    return NotFound();

                return Ok(history);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error retrieving order history for userId={userId}: {ex.Message}\n{ex.StackTrace}");
                return InternalServerError();
            }
        }
    }
}
