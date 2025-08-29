using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OrderManagement.DTOsModels
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T> { Success = false, Message = message, Data = default };
        }

        public static ApiResponse<T> Ok(T data, string message = null)
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data };
        }
    }
}