using System.Security.Claims;

namespace Dzaba.BasicAuthentication;

public interface IBasicAuthenticationHandlerService
{
    Task<bool> CheckPasswordAsync(BasicAuthenticationCredentials credentials);
    Task AddClaimsAsync(BasicAuthenticationCredentials credentials, ICollection<Claim> claims);
}
