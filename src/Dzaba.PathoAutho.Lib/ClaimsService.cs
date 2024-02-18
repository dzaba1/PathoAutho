using Dzaba.PathoAutho.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Dzaba.PathoAutho.Lib;

public interface IClaimsService
{
    IAsyncEnumerable<AppClaims> GetAppClaimsModelForUserAsync(string userId);
    Task<AppClaims> GetAppClaimsModelForUserAsync(string userId, Guid appId);  
    
}

internal sealed class ClaimsService : IClaimsService
{
    private readonly AppDbContext dbContext;
    private readonly ILogger<ClaimsService> logger;

    public ClaimsService(AppDbContext dbContext,
        ILogger<ClaimsService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.dbContext = dbContext;
        this.logger = logger;
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

    private async Task<AppClaims> GetAppClaimsModelForUserAsync(string userId, Model.Application app)
    {
        logger.LogDebug("Start getting permissions and roles for user with ID {UserId} and application {AppName}", userId, app.Name);

        var rolesQuery = from r in dbContext.PathoRoles
                         join ur in dbContext.PathoUserRoles on r.Id equals ur.RoleId
                         where ur.UserId == userId && r.ApplicationId == app.Id
                         select r;
        var roles = await rolesQuery.ToArrayAsync().ConfigureAwait(false);
        var rolesModel = roles.Select(r => new NamedEntity<int>
        {
            Id = r.Id,
            Name = r.Name
        }).ToArray();

        var permissionsQuery = from p in dbContext.Permissions
                               join up in dbContext.UserPermissions on p.Id equals up.PermissionId
                               where up.UserId == userId && p.ApplicationId == app.Id
                               select p;
        var permissions = await permissionsQuery.ToArrayAsync().ConfigureAwait(false);
        var permissionsModel = permissions.Select(r => new NamedEntity<int>
        {
            Id = r.Id,
            Name = r.Name
        }).ToArray();

        if (rolesModel.Any() || permissionsModel.Any())
        {
            return new AppClaims
            {
                Application = new NamedEntity<Guid>
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
