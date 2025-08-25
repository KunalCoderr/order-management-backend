using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services;
using OrderManagement.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace OrderManagement.Controllers
{
    public class UserController : ApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            // Manually instantiate required dependencies
            var dbContext = new OrderManagementEntities(); // your EF context
            IUserRepository userRepository = new UserRepository(dbContext);
            _userService = userService;
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(UserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required.");

            bool result = _userService.Register(dto);

            if (!result)
                return BadRequest("Username already exists.");

            return Ok("User registered successfully.");
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(UserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required.");

            var token = _userService.Login(dto);

            if (token == null)
                return Unauthorized();

            return Ok(new
            {
                Message = "Login successful",
                Token = token
            });
        }
    }
}
