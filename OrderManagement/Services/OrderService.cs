using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

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
            try
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
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Error placing order for user {request.UserId}: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public List<OrderHistory> GetOrderHistory(int userId)
        {
            try
            {
                var cachedOrders = _cacheService.Get<List<OrderHistory>>(ProductListCacheKey);
                if (cachedOrders != null && cachedOrders.Count > 0)
                {
                    cachedOrders = cachedOrders.FindAll(x => x.UserId == userId).ToList();
                    _cacheService.Set(ProductListCacheKey, cachedOrders, CacheExpiry);
                    return cachedOrders;
                }

                var orders = _orderRepository.GetOrdersByUser(userId);
                _cacheService.Set(ProductListCacheKey, orders, CacheExpiry);

                return orders;
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage(
                    $"Error retrieving order history for user {userId}: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}