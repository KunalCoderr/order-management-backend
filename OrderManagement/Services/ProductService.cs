using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
            try
            {
                var cachedProducts = _cacheService.Get<List<Product>>(ProductListCacheKey);
                if (cachedProducts != null)
                    return cachedProducts;

                var products = _repo.GetAll();
                _cacheService.Set(ProductListCacheKey, products, CacheExpiry);

                return products;
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error in GetAll() products: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public Product Get(int id)
        {
            try
            {
                return _repo.GetById(id);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error in product Get({id}) : {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void Create(ProductDTO dto)
        {
            try
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
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error in Create() product: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void Update(int id, ProductDTO dto)
        {
            try
            {
                var existingProduct = _repo.GetById(id);
                if (existingProduct == null)
                    throw new KeyNotFoundException($"Product with id {id} not found.");

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
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Error updating product {id}: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void Delete(int id)
        {
            try
            {
                var entity = _repo.GetById(id);
                if (entity == null)
                    throw new KeyNotFoundException($"Product with id {id} not found.");

                _repo.Delete(id);
                _repo.Save();

                _cacheService.Remove(ProductListCacheKey);
            }
            catch (DbUpdateException dbEx)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Unable to delete the product {id}: {dbEx.Message}\n{dbEx.StackTrace}");
                throw new InvalidOperationException(
                    "Unable to delete the product because it is referenced by other records.",
                    dbEx);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Unexpected error deleting product {id}: {ex.Message}\n{ex.StackTrace}");
                throw new ApplicationException("An error occurred while deleting the product.", ex);
            }
        }
    }
}