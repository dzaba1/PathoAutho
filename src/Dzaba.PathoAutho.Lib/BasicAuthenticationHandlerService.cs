using Dzaba.BasicAuthentication;
using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

internal sealed class BasicAuthenticationHandlerService : IBasicAuthenticationHandlerService
{
    private readonly IUserService userService;
    private readonly ILoginService loginService;
    private readonly IHttpClaimsService claimsService;

    public BasicAuthenticationHandlerService(IUserService userService,
            ILoginService loginService,
            IHttpClaimsService claimsService)
    {
        ArgumentNullException.ThrowIfNull(userService, nameof(userService));
        ArgumentNullException.ThrowIfNull(loginService, nameof(loginService));
        ArgumentNullException.ThrowIfNull(claimsService, nameof(claimsService));

        this.userService = userService;
        this.loginService = loginService;
        this.claimsService = claimsService;
    }

    public async Task AddClaimsAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext,
        ICollection<Claim> claims, object context)
    {
        var pathoIdentity = (PathoIdentityUser)context;

        claims.Add(new Claim("UserId", pathoIdentity.Id, ClaimValueTypes.String));
        claims.Add(new Claim(ClaimTypes.Email, pathoIdentity.Email, ClaimValueTypes.Email));

        await foreach (var role in claimsService.GetAppClaimsAsync(pathoIdentity, httpContext).ConfigureAwait(false))
        {
            claims.Add(role);
        }
    }

    public async Task<CheckPasswordResult> CheckPasswordAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext)
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
