using OrderManagement.DTOsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace OrderManagement.Controllers
{
    public class BaseApiController : ApiController
    {
        protected IHttpActionResult Success<T>(T data, string message = null)
        {
            var response = ApiResponse<T>.Ok(data, message);
            return Ok(response);
        }

        protected IHttpActionResult Fail<T>(string message)
        {
            var response = ApiResponse<T>.Fail(message);
            return Content(HttpStatusCode.BadRequest, response);
        }

        protected IHttpActionResult InternalError<T>(string message)
        {
            var response = ApiResponse<T>.Fail(message);
            return Content(HttpStatusCode.InternalServerError, response);
        }

        protected IHttpActionResult Unauthorized<T>(string message)
        {
            var response = ApiResponse<T>.Fail(message);
            return Content(HttpStatusCode.Unauthorized, response);
        }

    }
}
