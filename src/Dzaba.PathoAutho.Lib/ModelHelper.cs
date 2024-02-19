using Dzaba.PathoAutho.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

public interface IModelHelper
{
    Task<UserWithApplicationsPermission> GetForCurrentUserAsync(ClaimsPrincipal claimsPrincipal);
    Task<ApplicationPermissionsWithUser> GetForCurrentUserAsync(ClaimsPrincipal claimsPrincipal, Guid appId);
}

internal sealed class ModelHelper : IModelHelper
{
    private readonly IUserService userService;
    private readonly AppDbContext dbContext;
    private readonly IAuthHelper authHelper;
    private readonly IApplicationService applicationService;
    private readonly IRoleService roleService;

    public ModelHelper(IUserService userService,
        AppDbContext dbContext,
        IAuthHelper authHelper,
        IApplicationService applicationService,
        IRoleService roleService)
    {
        ArgumentNullException.ThrowIfNull(userService, nameof(userService));
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(authHelper, nameof(authHelper));
        ArgumentNullException.ThrowIfNull(applicationService, nameof(applicationService));
        ArgumentNullException.ThrowIfNull(roleService, nameof(roleService));

        this.userService = userService;
        this.dbContext = dbContext;
        this.authHelper = authHelper;
        this.applicationService = applicationService;
        this.roleService = roleService;
    }

    public async Task<UserWithApplicationsPermission> GetForCurrentUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal, nameof(claimsPrincipal));

        var user = await userService.FindUserByNameAsync(claimsPrincipal.Identity.Name).ConfigureAwait(false);
        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"User {claimsPrincipal.Identity.Name} doesn't exist.");
        }

        var admins = await dbContext.ApplicationAdmins
            .Where(a => a.UserId == user.Id)
            .Select(a => a.ApplicationId)
            .ToArrayAsync()
            .ConfigureAwait(false);

        var rolesQuery = from ur in dbContext.PathoUserRoles
                         join r in dbContext.PathoRoles on ur.RoleId equals r.Id
                         where ur.UserId == user.Id
                         select r;
        var roles = await rolesQuery.GroupBy(r => r.ApplicationId)
            .ToArrayAsync()
            .ConfigureAwait(false);

        var claims = await dbContext.PathoClaims
            .Where(a => a.UserId == user.Id)
            .GroupBy(r => r.ApplicationId)
            .ToArrayAsync()
            .ConfigureAwait(false);

        var appIds = admins
            .Union(roles.Select(r => r.Key))
            .Union(claims.Select(c => c.Key))
            .ToArray();

        var apps = dbContext.Applications
            .Where(a => appIds.Contains(a.Id))
            .ToAsyncEnumerable();

        await foreach (var app in apps.ConfigureAwait(false))
        {
            var test = new ApplicationPermissions
            {
                Application = app.ToModel(),
                IsAdmin = admins.Contains(app.Id),
                Roles = roles.Where(r => r.Key == app.Id)
                    .SelectMany(r => r)
                    .Select(r => r.ToModel())
                    .ToArray(),
                Claims = claims.Where(c => c.Key == app.Id)
                    .SelectMany(c => c)
                    .Select(c => c.ToModel())
                    .ToArray()
            };
        }

        return new UserWithApplicationsPermission
        {
            User = user.ToModel(),
            Permissions = await apps.Select(a => new ApplicationPermissions
            {
                Application = a.ToModel(),
                IsAdmin = admins.Contains(a.Id),
                Roles = roles.Where(r => r.Key == a.Id)
                    .SelectMany(r => r)
                    .Select(r => r.ToModel())
                    .ToArray(),
                Claims = claims.Where(c => c.Key == a.Id)
                    .SelectMany(c => c)
                    .Select(c => c.ToModel())
                    .ToArray()
            }).ToArrayAsync().ConfigureAwait(false),
            IsSuperAdmin = await authHelper.IsSuperAdminAsync(claimsPrincipal).ConfigureAwait(false)
        };
    }

    public async Task<ApplicationPermissionsWithUser> GetForCurrentUserAsync(ClaimsPrincipal claimsPrincipal, Guid appId)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal, nameof(claimsPrincipal));

        var user = await userService.FindUserByNameAsync(claimsPrincipal.Identity.Name).ConfigureAwait(false);
        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"User {claimsPrincipal.Identity.Name} doesn't exist.");
        }

        var app = await applicationService.GetApplicationAsync(appId).ConfigureAwait(false);
        if (app == null)
        {
            return null;
        }

        var claims = dbContext.PathoClaims
            .Where(c => c.ApplicationId == appId && c.UserId == user.Id)
            .ToAsyncEnumerable();

        var roles = from ur in dbContext.PathoUserRoles
                    join r in dbContext.PathoRoles on ur.RoleId equals r.Id
                    where ur.UserId == user.Id && r.ApplicationId == appId
                    select r;

        return new ApplicationPermissionsWithUser
        {
            User = user.ToModel(),
            IsAdmin = await roleService.IsApplicationAdminAsync(user.Id, appId),
            Application = app.ToModel(),
            Claims = await claims.Select(c => c.ToModel()).ToArrayAsync().ConfigureAwait(false),
            Roles = await roles.ToAsyncEnumerable().Select(r => r.ToModel()).ToArrayAsync().ConfigureAwait(false)
        };
    }
}
