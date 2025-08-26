using OrderManagement.DTOsModels;
using OrderManagement.Filters;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
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
            _orderService.PlaceOrder(request);
            return Ok(new { message = "Order placed successfully." });
        }

        [HttpGet()]
        [Route("history/{userId}")]
        public IHttpActionResult GetOrderHistory(int userId)
        {
            var history = _orderService.GetOrderHistory(userId);
            return Ok(history);
        }
    }
}
