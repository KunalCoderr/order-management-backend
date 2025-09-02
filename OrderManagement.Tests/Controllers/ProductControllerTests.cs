using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagement.Controllers;
using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Services.Contracts;

namespace OrderManagement.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockService;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockService = new Mock<IProductService>();
            _controller = new ProductController(_mockService.Object);
        }

        [Fact]
        public void GetAll_ShouldReturnSuccess_WhenProductsExist()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Apple", Price = 10 },
                new Product { Id = 2, Name = "Banana", Price = 5 }
            };

            _mockService.Setup(s => s.GetAll()).Returns(products);

            // Act
            var result = _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new { Message = "Products retrieved successfully.", Success = true });
        }

        [Fact]
        public void Get_ShouldReturnSuccess_WhenProductExists()
        {
            var product = new Product { Id = 1, Name = "Apple", Price = 10 };
            _mockService.Setup(s => s.Get(1)).Returns(product);

            var result = _controller.Get(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new { Message = "Product retrieved successfully.", Success = true });
        }

        [Fact]
        public void Get_ShouldReturnFail_WhenProductNotFound()
        {
            _mockService.Setup(s => s.Get(99)).Returns((Product)null);

            var result = _controller.Get(99);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Create_ShouldReturnSuccess_WhenProductIsValid()
        {
            var dto = new ProductDTO { Name = "Laptop", Price = 1000 };

            _controller.ModelState.Clear(); // Simulate valid model

            var result = _controller.Create(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Create_ShouldReturnFail_WhenModelStateIsInvalid()
        {
            var dto = new ProductDTO { Name = "", Price = 0 };

            _controller.ModelState.AddModelError("Name", "Required");

            var result = _controller.Create(dto);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Create_ShouldReturnFail_WhenProductNameOrPriceInvalid()
        {
            var dto = new ProductDTO { Name = "", Price = 0 };

            _controller.ModelState.Clear();

            var result = _controller.Create(dto);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
        }

        [Fact]
        public void Update_ShouldReturnSuccess_WhenValid()
        {
            var dto = new ProductDTO { Name = "Tablet", Price = 500 };
            var existingProduct = new Product { Id = 1, Name = "Tablet", Price = 400 };

            _mockService.Setup(s => s.Get(1)).Returns(existingProduct);

            _controller.ModelState.Clear();

            var result = _controller.Update(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Update_ShouldReturnNotFound_WhenProductDoesNotExist()
        {
            var dto = new ProductDTO { Name = "Tablet", Price = 500 };
            _mockService.Setup(s => s.Get(99)).Returns((Product)null);

            _controller.ModelState.Clear();

            var result = _controller.Update(99, dto);

            var notFoundResult = Assert.IsType<NotFoundResult>(result);
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Fact]
        public void Delete_ShouldReturnSuccess_WhenProductExists()
        {
            var existingProduct = new Product { Id = 1, Name = "Book", Price = 20 };
            _mockService.Setup(s => s.Get(1)).Returns(existingProduct);

            var result = _controller.Delete(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public void Delete_ShouldReturnFail_WhenProductNotFound()
        {
            _mockService.Setup(s => s.Get(99)).Returns((Product)null);

            var result = _controller.Delete(99);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
        }
    }
}
