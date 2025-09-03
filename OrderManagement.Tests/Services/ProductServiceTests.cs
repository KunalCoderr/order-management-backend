using FluentAssertions;
using Moq;
using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services;
using OrderManagement.Services.Contracts;

namespace OrderManagement.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly Mock<ICacheService> _mockCache;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockCache = new Mock<ICacheService>();
            _service = new ProductService(_mockRepo.Object, _mockCache.Object);
        }

        [Fact]
        public void GetAll_ShouldReturnFromCache_WhenCacheHasData()
        {
            // Arrange
            var cachedProducts = new List<Product> { new Product { Id = 1, Name = "CachedProduct" } };
            _mockCache.Setup(c => c.Get<List<Product>>("product_list")).Returns(cachedProducts);

            // Act
            var result = _service.GetAll();

            // Assert
            result.Should().BeEquivalentTo(cachedProducts);
            _mockRepo.Verify(r => r.GetAll(), Times.Never);
        }

        [Fact]
        public void GetAll_ShouldReturnFromRepoAndCache_WhenCacheIsEmpty()
        {
            // Arrange
            const string ProductListCacheKey = "product_list";
            var repoProducts = new List<Product> { new Product { Id = 2, Name = "RepoProduct" } };

            _mockCache.Setup(c => c.Get<List<Product>>(ProductListCacheKey)).Returns((List<Product>)null);
            _mockRepo.Setup(r => r.GetAll()).Returns(repoProducts);

            // Act
            var result = _service.GetAll();

            // Assert
            result.Should().BeEquivalentTo(repoProducts);
        }

        [Fact]
        public void Get_ShouldReturnProductById()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "TestProduct" };
            _mockRepo.Setup(r => r.GetById(1)).Returns(product);

            // Act
            var result = _service.Get(1);

            // Assert
            result.Should().BeEquivalentTo(product);
        }

        [Fact]
        public void Create_ShouldAddProductAndClearCache()
        {
            // Arrange
            var dto = new ProductDTO { Name = "NewProduct", Price = 10.5m, Description = "Desc" };

            // Act
            _service.Create(dto);

            // Assert
            _mockRepo.Verify(r => r.Add(It.Is<Product>(p => p.Name == dto.Name && p.Price == dto.Price)), Times.Once);
            _mockRepo.Verify(r => r.Save(), Times.Once);
            _mockCache.Verify(c => c.Remove("product_list"), Times.Once);
        }

        [Fact]
        public void Update_ShouldUpdateProductAndClearCache()
        {
            // Arrange
            var dto = new ProductDTO { Name = "UpdatedName", Price = 20m, Description = "Desc" };
            var existing = new Product { Id = 1, Name = "OldName" };
            _mockRepo.Setup(r => r.GetById(1)).Returns(existing);

            // Act
            _service.Update(1, dto);

            // Assert
            _mockRepo.Verify(r => r.Update(It.Is<Product>(p => p.Id == 1 && p.Name == dto.Name)), Times.Once);
            _mockRepo.Verify(r => r.Save(), Times.Once);
            _mockCache.Verify(c => c.Remove("product_list"), Times.Once);
        }

        [Fact]
        public void Update_ShouldThrow_WhenProductNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetById(1)).Returns((Product)null);

            // Act
            Action act = () => _service.Update(1, new ProductDTO());

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void Delete_ShouldDeleteProductAndClearCache()
        {
            // Arrange
            var existing = new Product { Id = 1, Name = "ToDelete" };
            _mockRepo.Setup(r => r.GetById(1)).Returns(existing);

            // Act
            _service.Delete(1);

            // Assert
            _mockRepo.Verify(r => r.Delete(1), Times.Once);
            _mockRepo.Verify(r => r.Save(), Times.Once);
            _mockCache.Verify(c => c.Remove("product_list"), Times.Once);
        }

        [Fact]
        public void Delete_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetById(1)).Returns((Product)null);

            // Act
            Action act = () => _service.Delete(1);

            // Assert
            act.Should()
               .Throw<ApplicationException>()
               .WithInnerException<KeyNotFoundException>()
               .WithMessage("Product with id 1 not found.");
        }

    }
}
