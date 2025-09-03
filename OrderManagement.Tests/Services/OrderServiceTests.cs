using FluentAssertions;
using Moq;
using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services;
using OrderManagement.Services.Contracts;

namespace OrderManagement.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<ICacheService> _mockCache;
        private readonly OrderService _service;

        public OrderServiceTests()
        { 
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockCache = new Mock<ICacheService>();
            _service = new OrderService(_mockOrderRepo.Object, _mockCache.Object);
        }
 
        [Fact]
        public void PlaceOrder_ShouldAddOrdersAndClearCache()
        {
            // Arrange
            var request = new PlaceOrderRequest
            {
                UserId = 1,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 100, Quantity = 2 },
                    new OrderItemDto { ProductId = 101, Quantity = 3 }
                }
            };

            // Act
            _service.PlaceOrder(request);

            // Assert
            _mockOrderRepo.Verify(r => r.AddOrder(It.Is<Order>(o =>
                o.UserId == request.UserId && o.ProductId == 100 && o.Quantity == 2)), Times.Once);

            _mockOrderRepo.Verify(r => r.AddOrder(It.Is<Order>(o =>
                o.UserId == request.UserId && o.ProductId == 101 && o.Quantity == 3)), Times.Once);

            _mockCache.Verify(c => c.Remove("order_list"), Times.Once);
        }

        [Fact]
        public void PlaceOrder_ShouldThrowAndLog_WhenExceptionOccurs()
        {
            // Arrange
            var request = new PlaceOrderRequest
            {
                UserId = 1,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 100, Quantity = 2 },
                    new OrderItemDto { ProductId = 101, Quantity = 3 }
                }
            };

            _mockOrderRepo
                .Setup(r => r.AddOrder(It.IsAny<Order>()))
                .Throws(new Exception("DB error"));

            // Act
            Action act = () => _service.PlaceOrder(request);

            // Assert
            act.Should()
                .Throw<Exception>()
                .WithMessage("DB error");
        }

        [Fact]
        public void GetOrderHistory_ShouldReturnFromCache_WhenCacheHasData()
        {
            // Arrange
            var userId = 1;
            var cachedOrders = new List<OrderHistory>
            {
                new OrderHistory { UserId = userId, OrderId = 1 },
                new OrderHistory { UserId = 2, OrderId = 2 } // different user, should be filtered
            };

            _mockCache.Setup(c => c.Get<List<OrderHistory>>("order_list")).Returns(cachedOrders);

            // Act
            var result = _service.GetOrderHistory(userId);

            // Assert
            // Ensure returned orders are only for requested user
            result.Should().OnlyContain(o => o.UserId == userId);

            // Repository should NOT be called when cache hits
            _mockOrderRepo.Verify(r => r.GetOrdersByUser(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetOrderHistory_ShouldReturnFromRepoAndCache_WhenCacheIsEmpty()
        {
            // Arrange
            var userId = 1;
            _mockCache.Setup(c => c.Get<List<OrderHistory>>("order_list")).Returns((List<OrderHistory>)null);

            var repoOrders = new List<OrderHistory>
            {
                new OrderHistory { UserId = userId, OrderId = 3 },
                new OrderHistory { UserId = userId, OrderId = 4 }
            };

            _mockOrderRepo.Setup(r => r.GetOrdersByUser(userId)).Returns(repoOrders);

            // Act
            var result = _service.GetOrderHistory(userId);

            // Assert
            result.Should().BeEquivalentTo(repoOrders);

            _mockCache.Verify(c => c.Set("order_list", repoOrders, It.IsAny<TimeSpan>()), Times.Once);
            _mockOrderRepo.Verify(r => r.GetOrdersByUser(userId), Times.Once);
        }

        [Fact]
        public void GetOrderHistory_ShouldThrowAndLog_WhenExceptionOccurs()
        {
            // Arrange
            var userId = 1;
            _mockCache.Setup(c => c.Get<List<OrderHistory>>("order_list")).Throws(new Exception("Cache failure"));

            // Act
            Action act = () => _service.GetOrderHistory(userId);

            // Assert
            act.Should().Throw<Exception>().WithMessage("Cache failure");
        }
    }
}

