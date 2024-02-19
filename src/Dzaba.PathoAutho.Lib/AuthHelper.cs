using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

public interface IAuthHelper
{
    Task CheckSuperOrAppAdminAsync(ClaimsPrincipal principal, Guid appId);
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
        ArgumentNullException.ThrowIfNull(principal, nameof(principal));

        if (principal.HasClaim(ClaimTypes.Role, RoleNames.SuperAdmin) ||
            principal.HasClaim(ClaimTypes.Role, RoleNames.AppAdmin))
        {
            return;
        }

        var user = await userManager.FindByNameAsync(principal.Identity.Name).ConfigureAwait(false);
        if (user != null)
        {
            var isAdmin = await roleService.IsApplicationAdminAsync(user.Id, appId)
            .ConfigureAwait(false);

            if (isAdmin)
            {
                return;
            }
        }

        throw new HttpResponseException(HttpStatusCode.Unauthorized, "Access denied.");
    }
}
