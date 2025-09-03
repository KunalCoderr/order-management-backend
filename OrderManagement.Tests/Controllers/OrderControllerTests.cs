using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagement.Controllers;
using OrderManagement.DTOsModels;
using OrderManagement.Services.Contracts;

namespace OrderManagement.Tests.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _controller = new OrderController(_mockOrderService.Object);
        }

        [Fact]
        public void PlaceOrder_ShouldReturnSuccess_WhenRequestIsValid()
        {
            var request = new PlaceOrderRequest
            {
                UserId = 1,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 1 },
                    new OrderItemDto { ProductId = 2, Quantity = 3 }
                }
            };

            _controller.ModelState.Clear(); // valid model

            var result = _controller.PlaceOrder(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public void PlaceOrder_ShouldReturnFail_WhenRequestIsNull()
        {
            var result = _controller.PlaceOrder(null);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
            objectResult.Value.Should().BeEquivalentTo(new { Message = "Order request is missing.", Success = false });
        }


        [Fact]
        public void PlaceOrder_ShouldReturnFail_WhenModelStateInvalid()
        {
            var request = new PlaceOrderRequest
            {
                UserId = 1,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 1 },
                    new OrderItemDto { ProductId = 2, Quantity = 3 }
                }
            };

            _controller.ModelState.AddModelError("ProductIds", "Required");

            var result = _controller.PlaceOrder(request);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
            objectResult.Value.Should().BeEquivalentTo(new { Message = "Invalid order data.", Success = false });
        }

        [Fact]
        public void PlaceOrder_ShouldReturnInternalError_OnException()
        {
            var request = new PlaceOrderRequest
            {
                UserId = 1,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 1 },
                    new OrderItemDto { ProductId = 2, Quantity = 3 }
                }
            };

            _mockOrderService.Setup(s => s.PlaceOrder(It.IsAny<PlaceOrderRequest>()))
                             .Throws(new Exception("Database error"));

            _controller.ModelState.Clear();

            var result = _controller.PlaceOrder(request);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public void GetOrderHistory_ShouldReturnSuccess_WhenOrdersExist()
        {
            var userId = 1;
            var history = new List<OrderHistory>
            {
                new OrderHistory
                {
                    OrderId = 1,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    UserName = "John Doe",
                    ProductId = 101,
                    ProductName = "Product A",
                    Quantity = 2,
                    Price = 100
                },
                new OrderHistory
                {
                    OrderId = 2,
                    UserId = userId,
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    UserName = "John Doe",
                    ProductId = 102,
                    ProductName = "Product B",
                    Quantity = 1,
                    Price = 200
                }
            };


            _mockOrderService.Setup(s => s.GetOrderHistory(userId)).Returns(history);

            var result = _controller.GetOrderHistory(userId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new { Data = history });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void GetOrderHistory_ShouldReturnFail_WhenUserIdInvalid(int userId)
        {
            var result = _controller.GetOrderHistory(userId);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
            objectResult.Value.Should().BeEquivalentTo(new { Message = "Invalid user ID.", Success = false });
        }

        [Fact]
        public void GetOrderHistory_ShouldReturnFail_WhenNoOrdersFound()
        {
            var userId = 99;
            var history = new List<OrderHistory>
            {
                new OrderHistory
                {
                    OrderId = 1,
                    UserId = 1,
                    OrderDate = DateTime.UtcNow,
                    UserName = "John Doe",
                    ProductId = 101,
                    ProductName = "Product A",
                    Quantity = 2,
                    Price = 100
                },
                new OrderHistory
                {
                    OrderId = 2,
                    UserId = 1,
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    UserName = "John Doe",
                    ProductId = 102,
                    ProductName = "Product B",
                    Quantity = 1,
                    Price = 200
                }
            };

            _mockOrderService.Setup(s => s.GetOrderHistory(userId)).Returns(history.FindAll(x => x.UserId == 99));

            var result = _controller.GetOrderHistory(userId);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void GetOrderHistory_ShouldReturnInternalError_OnException()
        {
            var userId = 1;
            _mockOrderService.Setup(s => s.GetOrderHistory(userId))
                             .Throws(new Exception("Failed to connect"));

            var result = _controller.GetOrderHistory(userId);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
        }
    }
}
