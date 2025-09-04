using OrderManagement.DTOsModels;
using OrderManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderManagement.Repositories.Contracts
{
    public interface IOrderRepository
    {
        void AddOrder(Order order);
        List<OrderHistory> GetOrdersByUser(int userId);
        Task AddOrdersAsync(List<Order> orders);
    }
}
