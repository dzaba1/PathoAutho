using Dzaba.BasicAuthentication;
using Dzaba.PathoAutho.Lib.Model;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

internal sealed class BasicAuthenticationHandlerService : IBasicAuthenticationHandlerService
{
    private readonly IUserService userService;
    private readonly ILoginService loginService;
    private readonly IRoleService rolesService;

    public BasicAuthenticationHandlerService(IUserService userService,
            ILoginService loginService,
            IRoleService rolesService)
    {
        ArgumentNullException.ThrowIfNull(userService, nameof(userService));
        ArgumentNullException.ThrowIfNull(loginService, nameof(loginService));
        ArgumentNullException.ThrowIfNull(rolesService, nameof(rolesService));

        this.userService = userService;
        this.loginService = loginService;
        this.rolesService = rolesService;
    }

    public async Task AddClaimsAsync(BasicAuthenticationCredentials credentials, ICollection<Claim> claims, object context)
    {
        var pathoIdentity = (PathoIdentityUser)context;

        claims.Add(new Claim("UserId", pathoIdentity.Id, ClaimValueTypes.String));
        claims.Add(new Claim(ClaimTypes.Email, pathoIdentity.Email, ClaimValueTypes.Email));

        await foreach (var role in rolesService.GetIdentityRolesAsync(pathoIdentity).ConfigureAwait(false))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
    }

    public async Task<CheckPasswordResult> CheckPasswordAsync(BasicAuthenticationCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials, nameof(credentials));

        var pathoIdentity = await userService.FindUserByNameAsync(credentials.UserName).ConfigureAwait(false);
        if (pathoIdentity == null)
        {
            return new CheckPasswordResult(false);
        }

        var result = await loginService.PasswordMatchAsync(pathoIdentity, credentials.Password).ConfigureAwait(false);
        return new CheckPasswordResult(result, pathoIdentity);
    }
}
