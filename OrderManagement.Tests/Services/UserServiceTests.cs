using FluentAssertions;
using Moq;
using OrderManagement.DTOsModels;
using OrderManagement.Models;
using OrderManagement.Repositories.Contracts;
using OrderManagement.Services;
using OrderManagement.Utilities;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace OrderManagement.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepo.Object);
        }

        [Fact]
        public void Register_ShouldReturnFalse_WhenUserAlreadyExists()
        {
            // Arrange
            var dto = new UserDTO { Username = "john", Password = "password" };
            _mockUserRepo.Setup(r => r.GetByUsername(dto.Username)).Returns(new User());

            // Act
            var result = _userService.Register(dto);

            // Assert
            result.Should().BeFalse();
            _mockUserRepo.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void Register_ShouldReturnTrue_WhenNewUserRegistered()
        {
            // Arrange
            var dto = new UserDTO { Username = "jane", Password = "password" };
            _mockUserRepo.Setup(r => r.GetByUsername(dto.Username)).Returns((User)null);

            // Act
            var result = _userService.Register(dto);

            // Assert
            result.Should().BeTrue();
            _mockUserRepo.Verify(r => r.Add(It.Is<User>(u => u.Username == dto.Username)), Times.Once);
            _mockUserRepo.Verify(r => r.Save(), Times.Once);
        }

        [Fact]
        public void Register_ShouldThrowException_WhenRepoThrows()
        {
            // Arrange
            var dto = new UserDTO { Username = "newuser", Password = "password" };
            _mockUserRepo.Setup(r => r.GetByUsername(dto.Username)).Throws(new Exception("Database down"));

            // Act
            Action act = () => _userService.Register(dto);

            // Assert
            act.Should().Throw<Exception>().WithMessage("*Database down*");
        }

        [Fact]
        public void Login_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var dto = new UserDTO { Username = "unknown", Password = "pass" };
            _mockUserRepo.Setup(r => r.GetByUsername(dto.Username)).Returns((User)null);

            // Act
            var token = _userService.Login(dto);

            // Assert
            token.Should().BeNull();
        }

        [Fact]
        public void Login_ShouldReturnToken_WhenUserFoundAndPasswordValid()
        {
            // Arrange
            var dto = new UserDTO { Username = "validuser", Password = "password" };
            PasswordHelper.CreatePasswordHash(dto.Password, out string hash, out string salt);

            var fakeUser = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            // Mock GetUserByUsername to return our fakeUser when username matches
            _mockUserRepo
                .Setup(repo => repo.GetByUsername(dto.Username))
                .Returns(fakeUser);

            // Act
            var token = _userService.Login(dto);

            // Assert login returned a token
            token.Should().NotBeNullOrEmpty();

            var session = _userService.GetSession(token);

            session.Should().NotBeNull();
            session.Username.Should().Be(dto.Username);
        }

        [Fact]
        public void Login_ShouldThrowException_WhenRepoThrows()
        {
            // Arrange
            var dto = new UserDTO { Username = "user", Password = "password" };
            _mockUserRepo.Setup(r => r.GetByUsername(dto.Username)).Throws(new Exception("Database failure"));

            // Act
            Action act = () => _userService.Login(dto);

            // Assert
            act.Should().Throw<Exception>().WithMessage("*Database failure*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsTokenValid_ShouldReturnFalse_WhenTokenIsNullOrEmpty(string token)
        {
            // Act
            var result = _userService.IsTokenValid(token);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsTokenValid_ShouldReturnTrue_WhenTokenIsValidAndNotExpired()
        {
            // Arrange
            var validToken = "valid-token";
            var expiry = DateTime.UtcNow.AddMinutes(30);

            var tokenField = typeof(UserService).GetField("_sessionStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var sessionStore = (ConcurrentDictionary<string, (int, string, DateTime)>)tokenField.GetValue(null);

            sessionStore[validToken] = (1, "user", expiry);

            // Act
            var result = _userService.IsTokenValid(validToken);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsTokenValid_ShouldReturnFalseAndRemove_WhenTokenIsExpired()
        {
            // Arrange
            var expiredToken = "expired-token";
            var expiry = DateTime.UtcNow.AddMinutes(-10);

            var tokenField = typeof(UserService).GetField("_sessionStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var sessionStore = (ConcurrentDictionary<string, (int, string, DateTime)>)tokenField.GetValue(null);

            sessionStore[expiredToken] = (1, "user", expiry);

            // Act
            var result = _userService.IsTokenValid(expiredToken);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Login_ShouldReturnNull_WhenPasswordIsIncorrect()
        {
            // Arrange
            var dto = new UserDTO { Username = "validuser", Password = "wrongpassword" };
            PasswordHelper.CreatePasswordHash("correctpassword", out string hash, out string salt);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepo.Setup(r => r.GetByUsername(dto.Username)).Returns(user);

            // Act
            var token = _userService.Login(dto);

            // Assert
            token.Should().BeNull();
        }

        [Fact]
        public void GetSession_ShouldReturnSessionInfo_WhenTokenValid()
        {
            // Arrange
            var dto = new UserDTO { Username = "validuser", Password = "password" };
            PasswordHelper.CreatePasswordHash(dto.Password, out string hash, out string salt);


            var fakeUser = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            // Mock GetUserByUsername to return our fakeUser when username matches
            _mockUserRepo
                .Setup(repo => repo.GetByUsername(dto.Username))
                .Returns(fakeUser);

            // Act
            var token = _userService.Login(dto);

            var session = _userService.GetSession(token);

            // Assert
            session.Should().NotBeNull();
            session.Username.Should().Be(dto.Username);
        }

        [Fact]
        public void GetSession_ShouldReturnNull_WhenTokenExpired()
        {
            // Arrange
            // Manually insert an expired token in _sessionStore

            var tokenField = typeof(UserService).GetField("_sessionStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var sessionStore = (System.Collections.Concurrent.ConcurrentDictionary<string, (int, string, DateTime)>)tokenField.GetValue(null);

            var expiredToken = "expired-token";
            sessionStore[expiredToken] = (1, "expiredUser", DateTime.UtcNow.AddMinutes(-10));

            // Act
            var session = _userService.GetSession(expiredToken);

            // Assert
            session.Should().BeNull();
            sessionStore.ContainsKey(expiredToken).Should().BeFalse(); // expired token removed
        }

        [Fact]
        public void IsTokenValid_ShouldCatchAndThrow_WhenExceptionOccurs()
        {
            // Arrange
            var tokenField = typeof(UserService).GetField("_sessionStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            // Backup original dictionary to restore later
            var originalStore = (ConcurrentDictionary<string, (int, string, DateTime)>)tokenField.GetValue(null);

            try
            {
                // Replace _sessionStore with faulty dictionary that throws exception
                tokenField.SetValue(null, new FaultyConcurrentDictionary());

                // Act
                var act = _userService.IsTokenValid("any-token");
                var actfd = _userService.IsTokenValid("any-token");

                // Assert
                act.Should().BeFalse();
            }
            finally
            {
                // Restore original dictionary after test
                tokenField.SetValue(null, originalStore);
            }
        }

        [Fact]
        public void GetSession_ShouldCatchAndThrow_WhenExceptionOccurs()
        {
            // Arrange
            var tokenField = typeof(UserService).GetField("_sessionStore", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var originalStore = (ConcurrentDictionary<string, (int, string, DateTime)>)tokenField.GetValue(null);

            try
            {
                tokenField.SetValue(null, new FaultyConcurrentDictionary());

                // Act
                var act = _userService.GetSession("any-token");

                // Assert
                act.Should().BeNull();
            }
            finally
            {
                tokenField.SetValue(null, originalStore);
            }
        }
    }

    public class FaultyConcurrentDictionary : ConcurrentDictionary<string, (int, string, DateTime)>
    {
        public new bool TryGetValue(string key, out (int, string, DateTime) value)
        {
            throw new Exception("Simulated exception");
        }
    }
}