using Dzaba.BasicAuthentication;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

internal sealed class BasicAuthenticationHandlerService : IBasicAuthenticationHandlerService
{
    private readonly IUserService userService;
    private readonly ILoginService loginService;

    public BasicAuthenticationHandlerService(IUserService userService,
            ILoginService loginService)
    {
        ArgumentNullException.ThrowIfNull(userService, nameof(userService));
        ArgumentNullException.ThrowIfNull(loginService, nameof(loginService));

        this.userService = userService;
        this.loginService = loginService;
    }

    public async Task AddClaimsAsync(BasicAuthenticationCredentials credentials, ICollection<Claim> claims)
    {
        var pathoIdentity = await userService.FindUserByNameAsync(credentials.UserName).ConfigureAwait(false);

        claims.Add(new Claim("UserId", pathoIdentity.Id, ClaimValueTypes.String));
        claims.Add(new Claim(ClaimTypes.Email, pathoIdentity.Email, ClaimValueTypes.Email));
    }

    public async Task<bool> CheckPasswordAsync(BasicAuthenticationCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials, nameof(credentials));

        var pathoIdentity = await userService.FindUserByNameAsync(credentials.UserName).ConfigureAwait(false);
        if (pathoIdentity == null)
        {
            return false;
        }

        return await loginService.PasswordMatchAsync(pathoIdentity, credentials.Password).ConfigureAwait(false);
    }
}
