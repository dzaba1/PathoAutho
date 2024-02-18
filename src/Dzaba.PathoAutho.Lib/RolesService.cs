using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Dzaba.PathoAutho.Lib;

public interface IRoleService
{
    Task<int> NewRoleAsync(Guid appId, string roleName);
    Task AssignUserToRoleAsync(string userId, int roleId);
    Task RemoveUserFromRoleAsync(string userId, int roleId);
    Task<PathoRole> GetRoleAsync(int roleId);
    Task RemoveRoleAsync(int roleId);
    IAsyncEnumerable<PathoRole> GetRolesAsync(string userId);
    IAsyncEnumerable<string> GetIdentityRolesAsync(PathoIdentityUser user);
    Task AssignUserToIdentiyRoleAsync(PathoIdentityUser user, string role);
    Task RemoveUserFromIdentiyRoleAsync(PathoIdentityUser user, string role);
}

internal class RolesService : IRoleService
{
    private readonly ILogger<RolesService> logger;
    private readonly UserManager<PathoIdentityUser> userManager;
    private readonly AppDbContext dbContext;

    public RolesService(ILogger<RolesService> logger,
        UserManager<PathoIdentityUser> userManager,
        AppDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        this.logger = logger;
        this.userManager = userManager;
        this.dbContext = dbContext;
    }

    public async Task AssignUserToIdentiyRoleAsync(PathoIdentityUser user, string role)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentException.ThrowIfNullOrWhiteSpace(role, nameof(role));

        var result = await userManager.AddToRoleAsync(user, role);
        result.EnsureSuccess();

        logger.LogInformation("Assigned {UserName} to identity role {IdentityRole}", user.UserName, role);
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

    public async IAsyncEnumerable<string> GetIdentityRolesAsync(PathoIdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));

        var list = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        foreach (var item in list)
        {
            yield return item;
        }
    }

    public async Task<PathoRole> GetRoleAsync(int roleId)
    {
        return await dbContext.PathoRoles.FirstOrDefaultAsync(r => r.Id == roleId)
            .ConfigureAwait(false);
    }

    public async IAsyncEnumerable<PathoRole> GetRolesAsync(string userId)
    {
        var query = from r in dbContext.PathoRoles
                   join ur in dbContext.PathoUserRoles on r.Id equals ur.RoleId
                   where ur.UserId == userId
                   select r;

        var col = query.ToAsyncEnumerable();
        await foreach (var item in col.ConfigureAwait(false))
        {
            yield return item;
        }
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

    public async Task RemoveRoleAsync(int roleId)
    {
        var role = await GetRoleAsync(roleId).ConfigureAwait(false);

        if (role == null)
        {
            return;
        }

        dbContext.PathoRoles.Remove(role);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Removed role with ID {RoleName} for application with ID {AppId}", role.Name, role.ApplicationId);
    }

    public async Task RemoveUserFromIdentiyRoleAsync(PathoIdentityUser user, string role)
    {
        ArgumentNullException.ThrowIfNull(user, nameof(user));
        ArgumentException.ThrowIfNullOrWhiteSpace(role, nameof(role));

        var result = await userManager.RemoveFromRoleAsync(user, role).ConfigureAwait(false);
        result.EnsureSuccess();

        logger.LogInformation("Removed {UserName} from identity role {IdentityRole}", user.UserName, role);
    }

    public async Task RemoveUserFromRoleAsync(string userId, int roleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        var link = await dbContext.PathoUserRoles.FirstOrDefaultAsync(r => r.UserId == userId && r.RoleId == roleId)
            .ConfigureAwait (false);

        if (link == null)
        {
            return;
        }

        dbContext.PathoUserRoles.Remove(link);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Removed user with ID {UserId} from role with ID {RoleId}", userId, roleId);
    }
}
