using OrderManagement.DTOsModels;
using OrderManagement.Models;
using System.Collections.Generic;

namespace OrderManagement.Repositories.Contracts
{
    public interface IOrderRepository
    {
        void AddOrder(Order order);
        List<OrderHistory> GetOrdersByUser(int userId);
    }
}
