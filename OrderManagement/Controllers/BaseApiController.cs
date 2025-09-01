using OrderManagement.DTOsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrderManagement.Controllers
{
    public class BaseApiController : ControllerBase
    {
        protected IActionResult Success<T>(T data, string message = null)
        {
            var response = ApiResponse<T>.Ok(data, message);
            return Ok(response);
        }

        protected IActionResult Fail<T>(string message)
        {
            var response = ApiResponse<T>.Fail(message);
            return BadRequest(response);
        }

        protected IActionResult InternalError<T>(string message)
        {
            var response = ApiResponse<T>.Fail(message);
            return StatusCode(500, response);
        }

        protected IActionResult Unauthorized<T>(string message)
        {
            var response = ApiResponse<T>.Fail(message);
            return Unauthorized(response);
        }

        protected IActionResult NotFound<T>(string message)
        {
            var response = ApiResponse<T>.Fail(message);
            return NotFound(response);
        }

    }
}
