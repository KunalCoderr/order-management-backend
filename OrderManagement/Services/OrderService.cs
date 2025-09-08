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
        private readonly IProductRepository _productRepository;

        private const string ProductListCacheKey = "order_list";
        private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

        public OrderService(IOrderRepository orderRepository, ICacheService cacheService, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _cacheService = cacheService;
            _productRepository = productRepository;
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

            int line = 2;

            try
            {
                var productDict = _productRepository.GetAll().ToDictionary(p => p.Id);
                var parsed = csv.GetRecords<CsvOrderRow>();

                var batch = new List<Order>(1000);
                int batchSize = 1000;

                foreach (var row in parsed)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(row.ProductId) ||
                            string.IsNullOrWhiteSpace(row.UserId) ||
                            string.IsNullOrWhiteSpace(row.Quantity) ||
                            string.IsNullOrWhiteSpace(row.OrderDate) ||
                            !int.TryParse(row.ProductId, out var productId) ||
                            !int.TryParse(row.UserId, out var userId) ||
                            !int.TryParse(row.Quantity, out var quantity) ||
                            !DateTime.TryParse(row.OrderDate, out var orderDate) ||
                            !productDict.TryGetValue(productId, out var product))
                        {
                            result.FailureCount++;
                            result.Errors.Add(new UploadError { Line = line, Reason = "Invalid or missing data." });
                            line++;
                            continue;
                        }

                        batch.Add(new Order
                        {
                            ProductId = productId,
                            UserId = userId,
                            Quantity = quantity,
                            OrderDate = orderDate
                        });

                        result.SuccessCount++;

                        if (batch.Count >= batchSize)
                        {
                            try
                            {
                                await _orderRepository.AddOrdersAsync(batch);
                            }
                            catch (Exception ex)
                            {
                                result.FailureCount += batch.Count;
                                result.Errors.Add(new UploadError { Line = line, Reason = $"Batch insert failed: {ex.Message}" });
                            }

                            batch.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount++;
                        result.Errors.Add(new UploadError { Line = line, Reason = ex.Message });
                    }

                    line++;
                }

                if (batch.Any())
                {
                    try
                    {
                        await _orderRepository.AddOrdersAsync(batch);
                    }
                    catch (Exception ex)
                    {
                        result.FailureCount += batch.Count;
                        result.Errors.Add(new UploadError { Line = line, Reason = $"Final batch insert failed: {ex.Message}" });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("CSV parsing failed.", ex);
            }
        }

    }
}