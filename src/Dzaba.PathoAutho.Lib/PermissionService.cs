using Dzaba.PathoAutho.Lib.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Dzaba.PathoAutho.Lib;

public interface IPermissionService
{
    Task<int> NewPermissionAsync(Guid appId, string permissionName);
    Task RemovePermissionAsync(int id);
    Task AssignUserToPermissionAsync(string userId, int permissionId);
    Task RemoveUserFromPermissionAsync(string userId, int permissionId);
    IAsyncEnumerable<Permission> GetPermissionsAsync(string userId);
    Task<Permission> GetPermissionAsync(int id);
}

internal sealed class PermissionService : IPermissionService
{
    private readonly ILogger<RolesService> logger;
    private readonly AppDbContext dbContext;

    public PermissionService(ILogger<RolesService> logger,
        AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        this.logger = logger;
        this.dbContext = dbContext;
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

    public async IAsyncEnumerable<Permission> GetPermissionsAsync(string userId)
    {
        var query = from p in dbContext.Permissions
                    join up in dbContext.UserPermissions on p.Id equals up.PermissionId
                    where up.UserId == userId
                    select p;

        var col = query.ToAsyncEnumerable();
        await foreach (var item in col.ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public async Task<Permission> GetPermissionAsync(int id)
    {
        return await dbContext.Permissions.FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
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

    public async Task RemovePermissionAsync(int id)
    {
        var permission = await GetPermissionAsync(id).ConfigureAwait(false);

        if (permission == null)
        {
            return;
        }

        dbContext.Permissions.Remove(permission);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Removed permission {PermissionName} for application with ID {AppId}", permission.Name, permission.ApplicationId);
    }

    public async Task RemoveUserFromPermissionAsync(string userId, int permissionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        var link = await dbContext.UserPermissions.FirstOrDefaultAsync(p => p.UserId == userId && p.PermissionId == permissionId)
            .ConfigureAwait(false);

        if (link == null)
        {
            return;
        }

        dbContext.UserPermissions.Remove(link);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Removed user with ID {UserId} from permission with ID {PermissionId}", userId, permissionId);
    }
}
