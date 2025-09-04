using CsvHelper;
using Microsoft.AspNetCore.Http;
using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICacheService _cacheService;
        private readonly IProductService _productService;

        private const string ProductListCacheKey = "order_list";
        private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

        public OrderService(IOrderRepository orderRepository, ICacheService cacheService, IProductService productService)
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
            _productService = productService;
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

        public async Task<UploadOrderResult> ProcessCsvAsync(IFormFile file)
        {
            var result = new UploadOrderResult();

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<Order>();
            int line = 1;

            try
            {
                var parsed = csv.GetRecords<dynamic>();

                var products = _productService.GetAll();

                foreach (var row in parsed)
                {
                    try
                    {
                        string ProductId = row.ProductId;
                        string UserId = row.UserId;
                        string Quantity = row.Quantity;
                        string dateTime = row.OrderDate;

                        if (string.IsNullOrWhiteSpace(ProductId) ||
                            string.IsNullOrWhiteSpace(UserId) ||
                            string.IsNullOrWhiteSpace(Quantity) ||
                            string.IsNullOrWhiteSpace(dateTime))
                        {
                            result.FailureCount++;
                            result.Errors.Add(new UploadError { Line = line, Reason = "Missing required fields." });
                            line++;
                            continue;
                        }

                        var product = products.FirstOrDefault(p => p.Id == int.Parse(ProductId));
                        if (product == null)
                        {
                            result.FailureCount++;
                            result.Errors.Add(new UploadError { Line = line, Reason = "Product does not exist: " + ProductId + "." });
                            line++;
                            continue;
                        }

                        records.Add(new Order
                        {
                            ProductId = int.Parse(ProductId),
                            UserId = int.Parse(UserId),
                            Quantity = int.Parse(Quantity),
                            OrderDate = DateTime.Parse(dateTime),
                        });

                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add(new UploadError { Line = line, Reason = ex.Message });
                    }

                    line++;
                }

                if (records.Any())
                {
                    await _orderRepository.AddOrdersAsync(records);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("CSV parsing failed: " + ex.Message);
            }
        }
    }
}