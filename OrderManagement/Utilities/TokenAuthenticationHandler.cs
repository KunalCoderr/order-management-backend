using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderManagement.Services.Contracts;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace OrderManagement.Utilities
{
    public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserService _userService;

        public TokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserService userService) // Inject your service
            : base(options, logger, encoder, clock)
        {
            _userService = userService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer "))
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));

            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (!_userService.IsTokenValid(token))
                return Task.FromResult(AuthenticateResult.Fail("Invalid or expired token"));

            var session = _userService.GetSession(token);
            if (session == null)
                return Task.FromResult(AuthenticateResult.Fail("Session not found or expired"));

            // Build claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
                new Claim(ClaimTypes.Name, session.Username)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
