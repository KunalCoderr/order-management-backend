using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagement.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICacheService _cacheService;

        private const string ProductListCacheKey = "product_list";
        private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

        public ProductService(IProductRepository repo, ICacheService cacheService)
        {
            _repo = repo;
            _cacheService = cacheService;
        }

        public IEnumerable<Product> GetAll()
        {
            var cachedProducts = _cacheService.Get<List<Product>>(ProductListCacheKey);
            if (cachedProducts != null)
                return cachedProducts;

            var products = _repo.GetAll();
            _cacheService.Set(ProductListCacheKey, products, CacheExpiry);

            return products;
        } 

        public Product Get(int id) => _repo.GetById(id);

        public void Create(ProductDTO dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            _repo.Add(product);
            _repo.Save();

            _cacheService.Remove(ProductListCacheKey);
        }

        public void Update(int id, ProductDTO dto)
        {
            var product = new Product
            {
                Id = id,
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            _repo.Update(product);
            _repo.Save();

            _cacheService.Remove(ProductListCacheKey);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
            _repo.Save();

            _cacheService.Remove(ProductListCacheKey);
        }
    }
}