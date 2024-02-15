using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Dzaba.PathoAutho.Lib
{
    public sealed class BasicAuthenticationHandler : IAuthenticationHandler
    {
        public static readonly string AuthenticationName = "BasicAuthentication";
        public static readonly string SchemeName = $"{AuthenticationName}Scheme";

        private readonly ILogger<BasicAuthenticationHandler> logger;
        private readonly IUserService userService;
        private readonly ILoginService loginService;
        private HttpContext context;

        public BasicAuthenticationHandler(ILogger<BasicAuthenticationHandler> logger,
            IUserService userService,
            ILoginService loginService)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(userService, nameof(userService));
            ArgumentNullException.ThrowIfNull(loginService, nameof(loginService));

            this.logger = logger;
            this.userService = userService;
            this.loginService = loginService;
        }

        public async Task<AuthenticateResult> AuthenticateAsync()
        {
            if (!context.Request.Headers.TryGetValue("Authorization", out var authorizationHeaderRaw))
            {
                return AuthenticateResult.Fail("Missing Authorization header.");
            }

            if (!AuthenticationHeaderValue.TryParse(authorizationHeaderRaw, out var authorizationHeader))
            {
                return AuthenticateResult.Fail("Error parsing Authorization header value.");
            }

            if (!string.Equals(authorizationHeader.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.Fail("Invalid Authorization header value.");
            }

            try
            {
                var credentialBytes = Convert.FromBase64String(authorizationHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];

                var pathoIdentity = await userService.FindUserByNameAsync(username).ConfigureAwait(false);
                if (pathoIdentity == null)
                {
                    return AuthenticateResult.Fail("Username or password is invalid");
                }

                if (await loginService.PasswordMatchAsync(pathoIdentity, password).ConfigureAwait(false))
                {
                    var claims = new[] {
                        new Claim("UserId", pathoIdentity.Id, ClaimValueTypes.String),
                        new Claim(ClaimTypes.Name, username, ClaimValueTypes.String),
                        new Claim(ClaimTypes.Email, pathoIdentity.Email, ClaimValueTypes.Email),
                    };

                    var identity = new ClaimsIdentity(claims, AuthenticationName);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, SchemeName);
                    return AuthenticateResult.Success(ticket);
                }

                return AuthenticateResult.Fail("Username or password is invalid");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error in Basic authentication.");
                return AuthenticateResult.Fail("Invalid Authorization header value.");
            }
        }

        public Task ChallengeAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties properties)
        {
            return Task.CompletedTask;
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            this.context = context;
            return Task.CompletedTask;
        }
    }
}
