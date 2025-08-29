using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace OrderManagement.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderManagementEntities _context;

        public OrderRepository(OrderManagementEntities context)
        {
            _context = context;
        }

        public void AddOrder(Order order)
        {
            _context.Orders.Add(order);
            _context.SaveChanges();
        }

        public List<OrderHistory> GetOrdersByUser(int userId)
        {
            var query = from order in _context.Orders
                        join user in _context.Users on order.UserId equals user.Id
                        join product in _context.Products on order.ProductId equals product.Id
                        where order.UserId == userId
                        select new OrderHistory
                        {
                            UserId = user.Id,
                            OrderId = order.Id,
                            OrderDate = order.OrderDate,
                            UserName = user.Username,
                            ProductId = product.Id,
                            ProductName = product.Name,
                            Quantity = order.Quantity,
                            Price = product.Price
                        };

            return query.ToList();
        }
    }
}