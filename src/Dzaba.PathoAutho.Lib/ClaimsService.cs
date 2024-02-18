using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Dzaba.PathoAutho.Lib;

public interface IClaimsService
{
    IAsyncEnumerable<AppClaims> GetAppClaimsModelForUserAsync(string userId);
    Task<AppClaims> GetAppClaimsModelForUserAsync(string userId, Guid appId);
    Task<int> NewRoleAsync(Guid appId, string roleName);
    Task<int> NewPermissionAsync(Guid appId, string permissionName);
    Task AssignUserToRoleAsync(string userId, int roleId);
    Task AssignUserToPermissionAsync(string userId, int permissionId);
    IAsyncEnumerable<string> GetIdentityRolesAsync(PathoIdentityUser user);
    Task AssignUserToIdentiyRoleAsync(PathoIdentityUser user, string role);
}

internal sealed class ClaimsService : IClaimsService
{
    private readonly AppDbContext dbContext;
    private readonly ILogger<ClaimsService> logger;
    private readonly UserManager<PathoIdentityUser> userManager;

    public ClaimsService(AppDbContext dbContext,
        ILogger<ClaimsService> logger,
        UserManager<PathoIdentityUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

        this.dbContext = dbContext;
        this.logger = logger;
        this.userManager = userManager;
    }

    public async Task AssignUserToIdentiyRoleAsync(PathoIdentityUser user, string role)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentException.ThrowIfNullOrWhiteSpace(role, nameof(role));

        var result = await userManager.AddToRoleAsync(user, role);
        result.EnsureSuccess();

        logger.LogInformation("Assigned {UserName} to identity role {IdentityRole}", user.UserName, role);
    }

    public async Task AssignUserToPermissionAsync(string userId, int permissionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        var exists = await dbContext.UserPermissions.AnyAsync(u => u.UserId == userId && u.PermissionId == permissionId)
            .ConfigureAwait(false);
        if (exists)
        {
            return;
        }

        var entity = new UserPermission
        {
            PermissionId = permissionId,
            UserId = userId,
        };
        dbContext.UserPermissions.Add(entity);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Assigned user with ID {UserId} to permission with ID {PermissionId}", userId, permissionId);
    }

    public async Task AssignUserToRoleAsync(string userId, int roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        var exists = await dbContext.PathoUserRoles.AnyAsync(u => u.UserId == userId && u.RoleId == roleId)
            .ConfigureAwait(false);
        if (exists)
        {
            return;
        }

        var entity = new PathoUserRole
        {
            RoleId = roleId,
            UserId = userId,
        };
        dbContext.PathoUserRoles.Add(entity);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Assigned user with ID {UserId} to role with ID {RoleId}", userId, roleId);
    }

    public async IAsyncEnumerable<AppClaims> GetAppClaimsModelForUserAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        await foreach (var app in dbContext.Applications.AsAsyncEnumerable().ConfigureAwait(false))
        {
            var model = await GetAppClaimsModelForUserAsync(userId, app).ConfigureAwait(false);
            if (model != null)
            {
                yield return model;
            }
        }
    }

    public async Task<AppClaims> GetAppClaimsModelForUserAsync(string userId, Guid appId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        var app = await dbContext.Applications.FirstOrDefaultAsync(a => a.Id == appId).ConfigureAwait(false);
        if (app == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"Application with ID {appId} doesn't exist");
        }

        return await GetAppClaimsModelForUserAsync(userId, app).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<string> GetIdentityRolesAsync(PathoIdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var list = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        foreach (var item in list)
        {
            yield return item;
        }
    }

    public async Task<int> NewPermissionAsync(Guid appId, string permissionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionName, nameof(permissionName));

        var exist = await dbContext.Permissions.AnyAsync(p => p.Name == permissionName && p.ApplicationId == appId)
            .ConfigureAwait(false);

        if (exist)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"Permission {permissionName} for application with ID {appId} already exists.");
        }

        var entity = new Permission
        {
            ApplicationId = appId,
            Name = permissionName
        };

        dbContext.Permissions.Add(entity);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Created a new permission {PermissionName} for application with ID {AppId}", permissionName, appId);

        return entity.Id;
    }

    public async Task<int> NewRoleAsync(Guid appId, string roleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleName, nameof(roleName));

        var exist = await dbContext.PathoRoles.AnyAsync(p => p.Name == roleName && p.ApplicationId == appId)
            .ConfigureAwait(false);

        if (exist)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"Role {roleName} for application with ID {appId} already exists.");
        }

        var entity = new PathoRole
        {
            ApplicationId = appId,
            Name = roleName
        };

        dbContext.PathoRoles.Add(entity);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Created a new role {RoleName} for application with ID {AppId}", roleName, appId);

        return entity.Id;
    }

    private async Task<AppClaims> GetAppClaimsModelForUserAsync(string userId, Model.Application app)
    {
        logger.LogDebug("Start getting permissions and roles for user with ID {UserId} and application {AppName}", userId, app.Name);

        var rolesQuery = from r in dbContext.PathoRoles
                         join ur in dbContext.PathoUserRoles on r.Id equals ur.RoleId
                         where ur.UserId == userId && r.ApplicationId == app.Id
                         select r;
        var roles = await rolesQuery.ToArrayAsync().ConfigureAwait(false);
        var rolesModel = roles.Select(r => new Claim
        {
            Id = r.Id,
            Name = r.Name
        }).ToArray();

        var permissionsQuery = from p in dbContext.Permissions
                               join up in dbContext.UserPermissions on p.Id equals up.PermissionId
                               where up.UserId == userId && p.ApplicationId == app.Id
                               select p;
        var permissions = await permissionsQuery.ToArrayAsync().ConfigureAwait(false);
        var permissionsModel = permissions.Select(r => new Claim
        {
            Id = r.Id,
            Name = r.Name
        }).ToArray();

        if (rolesModel.Any() || permissionsModel.Any())
        {
            return new AppClaims
            {
                Application = new Contracts.Application
                {
                    Id = app.Id,
                    Name = app.Name
                },
                Roles = rolesModel,
                Permissions = permissionsModel
            };
        }

        return null;
    }
}
