using OrderManagement.DTOsModels;
using OrderManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Services.Contracts
{
    public interface IProductService
    {
        IEnumerable<Product> GetAll();
        Product Get(int id);
        void Create(ProductDTO dto);
        void Update(int id, ProductDTO dto);
        void Delete(int id);
    }
}
