using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagement.DTOsModels
{
    public class PlaceOrderRequest
    {
        public int UserId { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}