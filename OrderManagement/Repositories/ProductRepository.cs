using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagement.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly OrderManagementEntities _context;

        public ProductRepository(OrderManagementEntities context)
        {
            _context = context;
        }

        public IEnumerable<Product> GetAll()
        {
            return _context.Products.ToList();
        }

        public Product GetById(int id)
        {
            return _context.Products.FirstOrDefault(p => p.Id == id);
        }

        public void Add(Product product)
        {
            _context.Products.Add(product);
        }

        public void Update(Product product)
        {
            var existing = _context.Products.Find(product.Id);
            if (existing != null)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.Description = product.Description;
                existing.CreatedAt = DateTime.UtcNow;
            }
        }

        public void Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }

}