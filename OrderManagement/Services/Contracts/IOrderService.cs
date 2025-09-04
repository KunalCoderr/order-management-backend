using Microsoft.AspNetCore.Http;
using OrderManagement.DTOsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagement.Services.Contracts
{
    public interface IOrderService
    {
        void PlaceOrder(PlaceOrderRequest request);
        List<OrderHistory> GetOrderHistory(int userId);
        Task<UploadOrderResult> ProcessCsvAsync(IFormFile file);
    }

}
