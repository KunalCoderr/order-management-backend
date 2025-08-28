using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Web.Http;

namespace OrderManagement.Controllers
{
    public class UserController : ApiController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            var dbContext = new OrderManagementEntities(); 
            IUserRepository userRepository = new UserRepository(dbContext);
            _userService = userService;
        }

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register(UserDTO dto)
        {
            try
            {
                if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Username) || !CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Password))
                    return BadRequest("Username and password are required.");

                dto.Username = CommonUtils.CommonUtils.ToTitleCase(dto.Username);
                CommonUtils.CommonUtils.LogMessage($"Attempted registration: {dto.Username}");

                bool result = _userService.Register(dto);

                if (!result)
                {
                    CommonUtils.CommonUtils.LogMessage($"Registration failed: Username '{dto.Username}' already exists.");
                    return BadRequest("Username already exists.");
                }

                CommonUtils.CommonUtils.LogMessage($"User '{dto.Username}' registered successfully.");

                return Ok("User registered successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error during registration: {ex.Message}\n{ex.StackTrace}");
                return InternalServerError();
            }
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(UserDTO dto)
        {
            try
            {
                if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Username) || !CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Password))
                    return BadRequest("Username and password are required.");

                CommonUtils.CommonUtils.LogMessage($"Login attempt for: {dto.Username}");

                var token = _userService.Login(dto);

                if (token == null)
                {
                    CommonUtils.CommonUtils.LogMessage($"CommonUtils: {dto.Username}");
                    return Unauthorized();
                }

                CommonUtils.CommonUtils.LogMessage($"Login successful for: {dto.Username}");

                return Ok(new
                {
                    Message = "Login successful",
                    Token = token
                });
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error during login for {dto?.Username}: {ex.Message}\n{ex.StackTrace}");

                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("session")]
        public IHttpActionResult GetSessionInfo([FromUri] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return Unauthorized();

                var session = _userService.GetSession(token);

                if (session == null)
                    return Unauthorized();

                return Ok(session);
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error in GetSessionInfo: {ex.Message}\n{ex.StackTrace}");

                return InternalServerError();
            }
        }
    }
}
