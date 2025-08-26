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
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICacheService _cacheService;

        private const string ProductListCacheKey = "order_list";
        private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

        public OrderService(IOrderRepository orderRepository, ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
        }

        public void PlaceOrder(PlaceOrderRequest request)
        {
            foreach (var item in request.Items)
            {
                var order = new Order
                {
                    UserId = request.UserId,
                    OrderDate = DateTime.Now,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                };

                _orderRepository.AddOrder(order);
            }

            _cacheService.Remove(ProductListCacheKey);
        }

        public List<OrderHistory> GetOrderHistory(int userId)
        {
            var cachedOrders = _cacheService.Get<List<OrderHistory>>(ProductListCacheKey);
            if (cachedOrders != null)
                return cachedOrders;

            var orders = _orderRepository.GetOrdersByUser(userId);
            _cacheService.Set(ProductListCacheKey, orders, CacheExpiry);

            return orders;
        }
    }
}