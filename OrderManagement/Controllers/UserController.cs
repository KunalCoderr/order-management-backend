using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services.Contracts;
using System;
using System.Web.Http;

namespace OrderManagement.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : BaseApiController
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
                    return Fail<object>("Username and password are required.");

                dto.Username = CommonUtils.CommonUtils.ToTitleCase(dto.Username);
                CommonUtils.CommonUtils.LogMessage($"Attempted registration: {dto.Username}");

                bool result = _userService.Register(dto);

                if (!result)
                {
                    CommonUtils.CommonUtils.LogMessage($"Registration failed: Username '{dto.Username}' already exists.");
                    return Fail<object>("Username already exists.");
                }

                CommonUtils.CommonUtils.LogMessage($"User '{dto.Username}' registered successfully.");

                return Success(result, "User registered successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error during registration: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred during Registration.");
            }
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(UserDTO dto)
        {
            try
            {
                if (!CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Username) || !CommonUtils.CommonUtils.IsNotNullOrEmpty(dto.Password))
                    return Fail<object>("Username and password are required.");

                CommonUtils.CommonUtils.LogMessage($"Login attempt for: {dto.Username}");

                var token = _userService.Login(dto);

                if (token == null)
                {
                    CommonUtils.CommonUtils.LogMessage($"CommonUtils: {dto.Username}");
                    return Unauthorized<object>("Invalid username or password.");
                }

                CommonUtils.CommonUtils.LogMessage($"Login successful for: {dto.Username}");

                return Success(new { Token = token }, "Login successful");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error during login for {dto?.Username}: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred during login.");
            }
        }

        [HttpGet]
        [Route("session")]
        public IHttpActionResult GetSessionInfo([FromUri] string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                    return Unauthorized<object>("Token is required.");

                var session = _userService.GetSession(token);

                if (session == null)
                    return Unauthorized<object>("Invalid or expired session token.");

                return Success(session, "Session retrieved successfully.");
            }
            catch (Exception ex)
            {
                CommonUtils.CommonUtils.LogMessage($"Unexpected error in GetSessionInfo: {ex.Message}\n{ex.StackTrace}");

                return InternalError<object>("An unexpected error occurred while retrieving session info.");
            }
        }
    }
}
