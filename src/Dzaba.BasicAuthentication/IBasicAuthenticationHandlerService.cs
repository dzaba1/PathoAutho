using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dzaba.BasicAuthentication;

/// <summary>
/// Declaration used for handling basic authentication.
/// </summary>
public interface IBasicAuthenticationHandlerService
{
    /// <summary>
    /// Checks if credentials match.
    /// </summary>
    /// <param name="credentials">Basic authentication credentials.</param>
    /// <param name="httpContext">HTTP context.</param>
    /// <returns>Check result.</returns>
    Task<CheckPasswordResult> CheckPasswordAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext);

    /// <summary>
    /// Adds or modifies additional claims.
    /// </summary>
    /// <param name="credentials">Basic authentication credentials.</param>
    /// <param name="httpContext">HTTP context.</param>
    /// <param name="claims">Claims collection to modify. Name claim is already set.</param>
    /// <param name="context">Handler context. It can be anything what you want. It's the same object return by <see cref="CheckPasswordResult"/>.</param>
    /// <returns>Task</returns>
    Task AddClaimsAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext, ICollection<Claim> claims, object context);

    /// <summary>
    /// Handles unauthorized.
    /// </summary>
    /// <param name="httpContext">HTTP context including response. The 401 code should be already set.</param>
    /// <param name="failReason">Original fail reason message.</param>
    /// <returns>Task</returns>
    Task HandleUnauthorizedAsync(HttpContext httpContext, string failReason);
}
