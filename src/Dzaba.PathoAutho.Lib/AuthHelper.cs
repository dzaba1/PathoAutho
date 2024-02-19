using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

public interface IAuthHelper
{
    Task CheckSuperOrAppAdminAsync(ClaimsPrincipal principal, Guid appId);
    Task<bool> IsSuperAdminAsync(ClaimsPrincipal principal);
    Task<bool> IsAppAdminAsync(ClaimsPrincipal principal, Guid appId);
}

internal sealed class AuthHelper : IAuthHelper
{
    private readonly IRoleService roleService;
    private readonly UserManager<PathoIdentityUser> userManager;

    public AuthHelper(IRoleService roleService,
        UserManager<PathoIdentityUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(roleService, nameof(roleService));
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

        this.roleService = roleService;
        this.userManager = userManager;
    }

    public async Task CheckSuperOrAppAdminAsync(ClaimsPrincipal principal, Guid appId)
    {
        if (principal == null)
        {
            throw new HttpResponseException(HttpStatusCode.Unauthorized, "Access denied.");
        }

        if (await IsSuperAdminAsync(principal).ConfigureAwait(false))
        {
            return;
        }

        if (await IsAppAdminAsync(principal, appId).ConfigureAwait(false))
        {
            return;
        }

        throw new HttpResponseException(HttpStatusCode.Unauthorized, "Access denied.");
    }

    public async Task<bool> IsAppAdminAsync(ClaimsPrincipal principal, Guid appId)
    {
        if (principal == null)
        {
            return false;
        }

        if (principal.HasClaim(ClaimTypes.Role, RoleNames.AppAdmin))
        {
            return true;
        }

        var user = await userManager.FindByNameAsync(principal.Identity.Name).ConfigureAwait(false);
        if (user != null)
        {
            return await roleService.IsApplicationAdminAsync(user.Id, appId)
                .ConfigureAwait(false);
        }

        return false;
    }

    public async Task<bool> IsSuperAdminAsync(ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            return false;
        }

        if (principal.HasClaim(ClaimTypes.Role, RoleNames.SuperAdmin))
        {
            return true;
        }

        var user = await userManager.FindByNameAsync(principal.Identity.Name).ConfigureAwait(false);
        if (user != null)
        {
            return await roleService.GetIdentityRolesAsync(user)
                .AnyAsync(r => r == RoleNames.SuperAdmin)
                .ConfigureAwait(false);
        }

        return false;
    }
}
