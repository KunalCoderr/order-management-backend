using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagement.Controllers;
using OrderManagement.DTOsModels;
using OrderManagement.Services.Contracts;

namespace OrderManagement.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UserController(_mockUserService.Object);
        }

        // ------------------ Register Tests ------------------

        [Fact]
        public void Register_ShouldReturnFail_WhenUsernameOrPasswordMissing()
        {
            // Arrange
            var dto = new UserDTO { Username = "", Password = "" };

            // Act
            var result = _controller.Register(dto);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400); // Fail<object>() likely returns BadRequest
            objectResult.Value.Should().BeEquivalentTo(new { Message = "Username and password are required.", Success = false });
        }

        [Fact]
        public void Register_ShouldReturnFail_WhenUsernameAlreadyExists()
        {
            // Arrange
            var dto = new UserDTO { Username = "existingUser", Password = "1234" };
            _mockUserService.Setup(x => x.Register(It.IsAny<UserDTO>())).Returns(false);

            // Act
            var result = _controller.Register(dto);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
            objectResult.Value.Should().BeEquivalentTo(new { Message = "Username already exists.", Success = false });
        }

        [Fact]
        public void Register_ShouldReturnSuccess_WhenRegistrationSuccessful()
        {
            // Arrange
            var dto = new UserDTO { Username = "newUser", Password = "1234" };
            _mockUserService.Setup(x => x.Register(It.IsAny<UserDTO>())).Returns(true);

            // Act
            var result = _controller.Register(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new { Message = "User registered successfully.", Success = true });
        }

        [Fact]
        public void Register_ShouldReturnInternalError_OnException()
        {
            // Arrange
            var dto = new UserDTO { Username = "test", Password = "1234" };
            _mockUserService.Setup(x => x.Register(It.IsAny<UserDTO>())).Throws(new Exception("DB error"));

            // Act
            var result = _controller.Register(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
            objectResult.Value.Should().BeEquivalentTo(new { Message = "An unexpected error occurred during Registration." });
        }

        // ------------------ Login Tests ------------------

        [Fact]
        public void Login_ShouldReturnFail_WhenUsernameOrPasswordMissing()
        {
            var dto = new UserDTO { Username = "", Password = "" };

            var result = _controller.Login(dto);

            var objectResult = Assert.IsType<BadRequestObjectResult>(result);
            objectResult.StatusCode.Should().Be(400);
            objectResult.Value.Should().BeEquivalentTo(new { Message = "Username and password are required." });
        }

        [Fact]
        public void Login_ShouldReturnUnauthorized_WhenLoginFails()
        {
            var dto = new UserDTO { Username = "user", Password = "wrongpass" };
            _mockUserService.Setup(x => x.Login(It.IsAny<UserDTO>())).Returns((string)null);

            var result = _controller.Login(dto);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public void Login_ShouldReturnSuccess_WhenLoginSucceeds()
        {
            var dto = new UserDTO { Username = "user", Password = "pass" };
            _mockUserService.Setup(x => x.Login(It.IsAny<UserDTO>())).Returns("fake-jwt-token");

            var result = _controller.Login(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new { Data = new { Token = "fake-jwt-token" } });
        }

        [Fact]
        public void Login_ShouldReturnInternalError_OnException()
        {
            var dto = new UserDTO { Username = "user", Password = "pass" };
            _mockUserService.Setup(x => x.Login(It.IsAny<UserDTO>())).Throws(new Exception("DB failure"));

            var result = _controller.Login(dto);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
        }

        // ------------------ GetSessionInfo Tests ------------------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void GetSessionInfo_ShouldReturnUnauthorized_WhenTokenMissing(string token)
        {
            var result = _controller.GetSessionInfo(token);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public void GetSessionInfo_ShouldReturnUnauthorized_WhenSessionInvalid()
        {
            var token = "invalid-token";
            _mockUserService.Setup(x => x.GetSession(token)).Returns((SessionInfo)(object)null);

            var result = _controller.GetSessionInfo(token);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            unauthorizedResult.StatusCode.Should().Be(401);
        }

        [Fact]
        public void GetSessionInfo_ShouldReturnSuccess_WhenSessionValid()
        {
            var token = "valid-token";
            var sessionObj = new SessionInfo
            {
                UserId = 1,
                Username = "john",
                Expiry = DateTime.UtcNow.AddHours(1)
            };

            _mockUserService
                .Setup(x => x.GetSession(It.IsAny<string>()))
                .Returns(sessionObj);

            var result = _controller.GetSessionInfo(token);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(new { Data = sessionObj });
        }

        [Fact]
        public void GetSessionInfo_ShouldReturnInternalError_OnException()
        {
            var token = "test-token";
            _mockUserService.Setup(x => x.GetSession(token)).Throws(new Exception("something failed"));

            var result = _controller.GetSessionInfo(token);

            var objectResult = Assert.IsType<ObjectResult>(result);
            objectResult.StatusCode.Should().Be(500);
        }
    }
}
