using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib.Model;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib;

public interface IModelHelper
{
    Task<UserWithApplicationsPermission> GetForCurrentUserAsync(ClaimsPrincipal claimsPrincipal);
    Task<ApplicationPermissionsWithUser> GetForCurrentUserAsync(ClaimsPrincipal claimsPrincipal, Guid appId);
    Task<ApplicationData> GetApplicationDataAsync(Guid appId);
    IAsyncEnumerable<ApplicationData> GetApplicationDataAsync(ClaimsPrincipal claimsPrincipal);
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

    public async Task<ApplicationData> GetApplicationDataAsync(Guid appId)
    {
        var app = await applicationService.GetApplicationAsync(appId).ConfigureAwait(false);
        if (app == null)
        {
            return null;
        }

        return await GetApplicationDataAsync(app).ConfigureAwait(false);
    }

    private async Task<ApplicationData> GetApplicationDataAsync(Application app)
    {
        var roles = dbContext.PathoRoles.Where(r => r.ApplicationId == app.Id).AsAsyncEnumerable();
        var rolesUser = from ur in dbContext.PathoUserRoles
                        join u in dbContext.Users on ur.UserId equals u.Id
                        join r in dbContext.PathoRoles on ur.RoleId equals r.Id
                        where r.ApplicationId == app.Id
                        select new { r, u };

        var claims = dbContext.PathoClaims.Where(c => c.ApplicationId == app.Id).AsAsyncEnumerable();

        return new ApplicationData
        {
            Application = app.ToModel(),
            Admins = await roleService.GetAdmins(app.Id).Select(u => u.ToModel()).ToArrayAsync().ConfigureAwait(false),
            Roles = await roles.Select(r => r.ToModel()).ToArrayAsync().ConfigureAwait(false),
            RoleAssingments = await rolesUser.AsAsyncEnumerable().Select(ur => new UserRoleAssingment
            {
                Role = ur.r.ToModel(),
                User = ur.u.ToModel(),
            }).ToArrayAsync().ConfigureAwait(false),
            Claims = await claims.Select(c => c.ToModel()).ToArrayAsync().ConfigureAwait(false)
        };
    }

    public async IAsyncEnumerable<ApplicationData> GetApplicationDataAsync(ClaimsPrincipal claimsPrincipal)
    {
        var apps = dbContext.Applications.AsAsyncEnumerable();

        await foreach (var app in apps.ConfigureAwait(false))
        {
            if (await authHelper.IsSuperAdminAsync(claimsPrincipal).ConfigureAwait(false) &&
                await authHelper.IsAppAdminAsync(claimsPrincipal, app.Id).ConfigureAwait(false))
            {
                yield return await GetApplicationDataAsync(app);
            }
        }
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
