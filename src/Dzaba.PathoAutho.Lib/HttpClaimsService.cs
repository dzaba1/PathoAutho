using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

internal interface IHttpClaimsService
{
    IAsyncEnumerable<Claim> GetAppClaimsAsync(PathoIdentityUser user, HttpContext httpContext);
}

internal class HttpClaimsService : IHttpClaimsService
{
    private readonly IRoleService roleService;

    public HttpClaimsService(IRoleService roleService)
    {
        ArgumentNullException.ThrowIfNull(roleService, nameof(roleService));

        this.roleService = roleService;
    }

    public async IAsyncEnumerable<Claim> GetAppClaimsAsync(PathoIdentityUser user, HttpContext httpContext)
    {
        await foreach (var role in roleService.GetIdentityRolesAsync(user).ConfigureAwait(false))
        {
            yield return new Claim(ClaimTypes.Role, role);
        }

        if (!TryGetAppId(httpContext, out var appId))
        {
            yield break;
        }

        var isAdmin = await roleService.IsApplicationAdminAsync(user.Id, appId)
            .ConfigureAwait(false);
        if (isAdmin)
        {
            yield return new Claim(ClaimTypes.Role, RoleNames.AppAdmin);
        }
    }

    private bool TryGetAppId(HttpContext httpContext, out Guid id)
    {
        id = Guid.Empty;
        return false;
    }
}
