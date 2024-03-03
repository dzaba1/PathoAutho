using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
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

        try
        {
            if (!BasicAuthenticationCredentials.TryParseHeader(authorizationHeaderRaw, out var credentials))
            {
                return Fail("Invalid Authorization header value.");
            }

            var checkPasswordResult = await handlerService.CheckPasswordAsync(credentials, Context).ConfigureAwait(false);

            if (!checkPasswordResult.IsSuccess)
            {
                return Fail(checkPasswordResult.FailReason);
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
            await handlerService.HandleUnauthorizedAsync(Context, failReason).ConfigureAwait(false);
        }
    }
}
