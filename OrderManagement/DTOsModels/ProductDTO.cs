using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagement.DTOsModels
{
    public class ProductDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}