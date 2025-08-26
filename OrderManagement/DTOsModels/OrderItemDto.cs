using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagement.DTOsModels
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}