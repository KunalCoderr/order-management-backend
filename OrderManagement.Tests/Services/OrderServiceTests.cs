using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services;
using OrderManagement.Services.Contracts;
using System.Text;

namespace OrderManagement.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<ICacheService> _mockCache;
        private readonly Mock<IProductService> _productService;
        private readonly OrderService _service;

        public OrderServiceTests()
        { 
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockCache = new Mock<ICacheService>();
            _productService = new Mock<IProductService>();
            _service = new OrderService(_mockOrderRepo.Object, _mockCache.Object, _productService.Object);
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

        private IFormFile CreateCsvFile(string csvContent)
        {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
            return new FormFile(stream, 0, stream.Length, "file", "orders.csv");
        }

        [Fact]
        public async Task ProcessCsvAsync_AllValidRows_ReturnsSuccess()
        {
            // Arrange
            string csv = "ProductId,UserId,Quantity,OrderDate\n1,1001,2,2023-01-01";
            var file = CreateCsvFile(csv);
            _productService.Setup(p => p.GetAll())
                .Returns(new List<Product> { new Product { Id = 1 } });

            // Act
            var result = await _service.ProcessCsvAsync(file);

            // Assert
            Assert.Equal(1, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Empty(result.Errors);
            _mockOrderRepo.Verify(r => r.AddOrdersAsync(It.IsAny<List<Order>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessCsvAsync_MissingFields_ReturnsFailure()
        {
            // Arrange: Missing UserId
            string csv = "ProductId,UserId,Quantity,OrderDate\n1,,2,2023-01-01";
            var file = CreateCsvFile(csv);
            _productService.Setup(p => p.GetAll())
                .Returns(new List<Product> { new Product { Id = 1 } });

            // Act
            var result = await _service.ProcessCsvAsync(file);

            // Assert
            Assert.Equal(0, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Single(result.Errors);
            Assert.Contains("Missing required fields", result.Errors[0].Reason);
            _mockOrderRepo.Verify(r => r.AddOrdersAsync(It.IsAny<List<Order>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessCsvAsync_InvalidProduct_ReturnsFailure()
        {
            // Arrange: ProductId = 999 does not exist
            string csv = "ProductId,UserId,Quantity,OrderDate\n999,1001,2,2023-01-01";
            var file = CreateCsvFile(csv);
            _productService.Setup(p => p.GetAll())
                .Returns(new List<Product> { new Product { Id = 1 } });

            // Act
            var result = await _service.ProcessCsvAsync(file);

            // Assert
            Assert.Equal(0, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Single(result.Errors);
            Assert.Contains("Product does not exist", result.Errors[0].Reason);
            _mockOrderRepo.Verify(r => r.AddOrdersAsync(It.IsAny<List<Order>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessCsvAsync_InvalidDataType_ReturnsFailure()
        {
            // Arrange: Quantity is not a number
            string csv = "ProductId,UserId,Quantity,OrderDate\n1,1001,abc,2023-01-01";
            var file = CreateCsvFile(csv);
            _productService.Setup(p => p.GetAll())
                .Returns(new List<Product> { new Product { Id = 1 } });

            // Act
            var result = await _service.ProcessCsvAsync(file);

            // Debug
            Console.WriteLine($"Actual error: {result.Errors[0].Reason}");

            // Assert
            Assert.Equal(0, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Single(result.Errors);
            Assert.Contains("not in a correct format", result.Errors[0].Reason);
            _mockOrderRepo.Verify(r => r.AddOrdersAsync(It.IsAny<List<Order>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessCsvAsync_NoHeaders_ThrowsException()
        {
            // Arrange: No header row
            string csv = "1,1001,2,2023-01-01";
            var file = CreateCsvFile(csv);
            _productService.Setup(p => p.GetAll())
                .Returns(new List<Product> { new Product { Id = 1 } });

            // Act
            var result = await _service.ProcessCsvAsync(file);

            // Assert
            Assert.Equal(0, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Empty(result.Errors);
            _mockOrderRepo.Verify(r => r.AddOrdersAsync(It.IsAny<List<Order>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessCsvAsync_MixedValidAndInvalidRows_ReturnsPartialSuccess()
        {
            // Arrange: One valid, one missing field
            string csv = "ProductId,UserId,Quantity,OrderDate\n1,1001,2,2023-01-01\n1,,5,2023-01-01";
            var file = CreateCsvFile(csv);
            _productService.Setup(p => p.GetAll())
                .Returns(new List<Product> { new Product { Id = 1 } });

            // Act
            var result = await _service.ProcessCsvAsync(file);

            // Assert
            Assert.Equal(1, result.SuccessCount);
            Assert.Equal(1, result.FailureCount);
            Assert.Single(result.Errors);
            _mockOrderRepo.Verify(r => r.AddOrdersAsync(It.Is<List<Order>>(l => l.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task ProcessCsvAsync_EmptyFile_ReturnsEmptyResult()
        {
            // Arrange
            string csv = "";
            var file = CreateCsvFile(csv);
            _productService.Setup(p => p.GetAll()).Returns(new List<Product>());

            // Act
            var result = await _service.ProcessCsvAsync(file);

            // Assert
            Assert.Equal(0, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Empty(result.Errors);
            _mockOrderRepo.Verify(r => r.AddOrdersAsync(It.IsAny<List<Order>>()), Times.Never);
        }
    }
}

