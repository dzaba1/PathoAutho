using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Dzaba.BasicAuthentication;

internal sealed class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static readonly string FailReasonKeyName = "FailReason";

    private readonly ILogger<BasicAuthenticationHandler> logger;
    private readonly IBasicAuthenticationHandlerService handlerService;
    private string failReason;

    public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IBasicAuthenticationHandlerService handlerService,
        ILogger<BasicAuthenticationHandler> logger)
        : base(options, loggerFactory, encoder)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(handlerService, nameof(handlerService));

        this.logger = logger;
        this.handlerService = handlerService;
    }

    private AuthenticateResult Fail(string msg)
    {
        failReason = msg;
        return AuthenticateResult.Fail(msg);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        failReason = null;

        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeaderRaw))
        {
            return Fail("Missing Authorization header.");
        }

        if (!AuthenticationHeaderValue.TryParse(authorizationHeaderRaw, out var authorizationHeader))
        {
            return Fail("Error parsing Authorization header value.");
        }

        if (!string.Equals(authorizationHeader.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
        {
            return Fail("Invalid Authorization header value.");
        }

        try
        {
            var credentialBytes = Convert.FromBase64String(authorizationHeader.Parameter);
            var credentialsArray = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var credentials = new BasicAuthenticationCredentials(credentialsArray[0], credentialsArray[1]);

            var checkPasswordResult = await handlerService.CheckPasswordAsync(credentials, Context).ConfigureAwait(false);

            if (!checkPasswordResult.Success)
            {
                return Fail("Username or password is invalid");
            }

            var claims = new List<Claim>(1)
                    {
                        new Claim(ClaimTypes.Name, credentials.UserName, ClaimValueTypes.String),
                    };

            await handlerService.AddClaimsAsync(credentials, Context, claims, checkPasswordResult.Context)
                .ConfigureAwait(false);

            var identity = new ClaimsIdentity(claims, Constants.AuthenticationName);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Constants.SchemeName);
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error in Basic authentication.");
            return Fail("Invalid Authorization header value.");
        }
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await base.HandleChallengeAsync(properties).ConfigureAwait(false);

        if (Response.StatusCode == StatusCodes.Status401Unauthorized &&
            !string.IsNullOrWhiteSpace(failReason))
        {
            await Response.WriteAsync(failReason).ConfigureAwait(false);
        }
    }
}
