using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dzaba.BasicAuthentication;

public interface IBasicAuthenticationHandlerService
{
    Task<CheckPasswordResult> CheckPasswordAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext);
    Task AddClaimsAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext, ICollection<Claim> claims, object context);
}
