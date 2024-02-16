﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace Dzaba.BasicAuthentication;

internal sealed class BasicAuthenticationHandler : IAuthenticationHandler
{
    private readonly ILogger<BasicAuthenticationHandler> logger;
    private readonly IBasicAuthenticationHandlerService handlerService;
    private HttpContext context;

    public BasicAuthenticationHandler(ILogger<BasicAuthenticationHandler> logger,
        IBasicAuthenticationHandlerService handlerService)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(handlerService, nameof(handlerService));

        this.logger = logger;
        this.handlerService = handlerService;
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
            var credentialsArray = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var credentials = new BasicAuthenticationCredentials(credentialsArray[0], credentialsArray[1]);

            if (await handlerService.CheckPasswordAsync(credentials).ConfigureAwait(false))
            {
                var claims = new List<Claim>(1)
                {
                    new Claim(ClaimTypes.Name, credentials.UserName, ClaimValueTypes.String),
                };

                var identity = new ClaimsIdentity(claims, Constants.AuthenticationName);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Constants.SchemeName);
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