using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
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
        if (httpContext.Request.Path.StartsWithSegments("/Application"))
        {
            if (TryGetAppIdFromRouteValues(httpContext.Request.RouteValues, "appId", out id))
            {
                return true;
            }
        }

        id = Guid.Empty;
        return false;
    }

    private bool TryGetAppIdFromRouteValues(IReadOnlyDictionary<string, object> routeValues, string keyName, out Guid id)
    {
        if (routeValues != null && routeValues.TryGetValue(keyName, out var valueRaw))
        {
            if (valueRaw is Guid)
            {
                id = (Guid)valueRaw;
                return true;
            }

            if (valueRaw is string && Guid.TryParse((string)valueRaw, out id))
            {
                return true;
            }
        }

        id = Guid.Empty;
        return false;
    }
}
