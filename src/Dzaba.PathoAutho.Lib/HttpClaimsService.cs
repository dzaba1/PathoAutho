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
    private readonly IClaimService claimService;

    public HttpClaimsService(IRoleService roleService,
        IClaimService claimService)
    {
        ArgumentNullException.ThrowIfNull(roleService, nameof(roleService));
        ArgumentNullException.ThrowIfNull(claimService, nameof(claimService));

        this.roleService = roleService;
        this.claimService = claimService;
    }

    public async IAsyncEnumerable<Claim> GetAppClaimsAsync(PathoIdentityUser user, HttpContext httpContext)
    {
        await foreach (var role in roleService.GetIdentityRolesAsync(user).ConfigureAwait(false))
        {
            yield return new Claim(ClaimTypes.Role, role);
        }

        var appId = await TryGetAppIdAsync(httpContext)
            .ConfigureAwait(false);

        if (appId == null)
        {
            yield break;
        }

        var isAdmin = await roleService.IsApplicationAdminAsync(user.Id, appId.Value)
            .ConfigureAwait(false);
        if (isAdmin)
        {
            yield return new Claim(ClaimTypes.Role, RoleNames.AppAdmin);
        }
    }

    private async Task<Guid?> TryGetAppIdAsync(HttpContext httpContext)
    {
        var request = httpContext.Request;

        if (request.Path.StartsWithSegments("/Application"))
        {
            var appId = TryGetFromRouteValues<Guid>(request.RouteValues, "appId", s =>
            {
                if (Guid.TryParse(s, out var id))
                {
                    return id;
                }
                return null;
            });

            if (appId != null)
            {
                return appId;
            }
        }

        if (request.Path.StartsWithSegments("/Role"))
        {
            var roleId = TryGetFromRouteValues<int>(request.RouteValues, "roleId", s =>
            {
                if (int.TryParse(s, out var id))
                {
                    return id;
                }
                return null;
            });

            if (roleId != null)
            {
                var role = await roleService.GetRoleAsync(roleId.Value)
                    .ConfigureAwait(false);
                if (role != null)
                {
                    return role.ApplicationId;
                }
            }
        }

        if (request.Path.StartsWithSegments("/Claim"))
        {
            var claimId = TryGetFromRouteValues<int>(request.RouteValues, "claimId", s =>
            {
                if (int.TryParse(s, out var id))
                {
                    return id;
                }
                return null;
            });

            if (claimId != null)
            {
                var claim = await claimService.GetClaimAsync(claimId.Value)
                    .ConfigureAwait(false);
                if (claim != null)
                {
                    return claim.ApplicationId;
                }
            }
        }

        return null;
    }

    private T? TryGetFromRouteValues<T>(IReadOnlyDictionary<string, object> routeValues, string keyName,
        Func<string, T?> parser)
        where T : struct
    {
        if (routeValues != null && routeValues.TryGetValue(keyName, out var valueRaw))
        {
            if (valueRaw is T)
            {
                return (T)valueRaw;
            }

            if (valueRaw is string)
            {
                return parser((string)valueRaw);
            }
        }

        return null;
    }
}
