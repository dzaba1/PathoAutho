using System.Security.Claims;

namespace Dzaba.BasicAuthentication;

public interface IBasicAuthenticationHandlerService
{
    Task<CheckPasswordResult> CheckPasswordAsync(BasicAuthenticationCredentials credentials);
    Task AddClaimsAsync(BasicAuthenticationCredentials credentials, ICollection<Claim> claims, object context);
}
