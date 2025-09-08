using FluentAssertions;
using Moq;
using OrderManagement.Filters;
using OrderManagement.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace OrderManagement.Tests.Filters
{
    public class AuthorizeSessionAttributeTests
    {
        private readonly AuthorizeSessionAttribute _attribute;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly HttpActionContext _actionContext;

        public AuthorizeSessionAttributeTests()
        {
            _attribute = new AuthorizeSessionAttribute();
            _userServiceMock = new Mock<IUserService>();

            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();

            var controllerContext = new HttpControllerContext
            {
                Configuration = config,
                Request = request
            };

            _actionContext = new HttpActionContext
            {
                ControllerContext = controllerContext,
                ActionDescriptor = new Mock<HttpActionDescriptor>().Object
            };

            // Setup DependencyResolver mock to return IUserService mock
            var dependencyResolverMock = new Mock<System.Web.Http.Dependencies.IDependencyResolver>();
            dependencyResolverMock
                .Setup(dr => dr.GetService(typeof(IUserService)))
                .Returns(_userServiceMock.Object);

            config.DependencyResolver = dependencyResolverMock.Object;
        }

        [Fact]
        public void OnAuthorization_NoAuthorizationHeader_ShouldSetUnauthorizedResponse()
        {
            // Arrange: no auth header

            // Act
            _attribute.OnAuthorization(_actionContext);

            // Assert
            _actionContext.Response.Should().NotBeNull();
            _actionContext.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void OnAuthorization_AuthorizationSchemeNotBearer_ShouldSetUnauthorizedResponse()
        {
            // Arrange
            _actionContext.Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", "some-token");

            // Act
            _attribute.OnAuthorization(_actionContext);

            // Assert
            _actionContext.Response.Should().NotBeNull();
            _actionContext.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void OnAuthorization_BearerSchemeButEmptyToken_ShouldSetUnauthorizedResponse()
        {
            // Arrange
            _actionContext.Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "");

            // Act
            _attribute.OnAuthorization(_actionContext);

            // Assert
            _actionContext.Response.Should().NotBeNull();
            _actionContext.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void OnAuthorization_InvalidToken_ShouldSetUnauthorizedResponse()
        {
            // Arrange
            var token = "invalid-token";
            _actionContext.Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            _userServiceMock.Setup(u => u.IsTokenValid(token)).Returns(false);

            // Act
            _attribute.OnAuthorization(_actionContext);

            // Assert
            _actionContext.Response.Should().NotBeNull();
            _actionContext.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void OnAuthorization_ValidToken_ShouldNotSetResponse()
        {
            // Arrange
            var token = "valid-token";
            _actionContext.Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            _userServiceMock.Setup(u => u.IsTokenValid(token)).Returns(true);

            // Act
            _attribute.OnAuthorization(_actionContext);

            // Assert
            _actionContext.Response.Should().BeNull(); // Means authorization succeeded
        }
    }
}
