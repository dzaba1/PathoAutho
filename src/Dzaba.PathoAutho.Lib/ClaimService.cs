using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Dzaba.PathoAutho.Lib;

public interface IClaimService
{
    Task<int> SetClaimAsync(Guid appId, string userName, string type, string value);
    Task RemoveClaimAsync(int claimId);
    IAsyncEnumerable<PathoClaim> GetClaimsAsync(string userId);
    IAsyncEnumerable<PathoClaim> GetClaimsAsync(string userId, Guid appId);
    Task<PathoClaim> GetClaimAsync(int id);
}

internal sealed class ClaimService : IClaimService
{
    private readonly ILogger<ClaimService> logger;
    private readonly AppDbContext dbContext;
    private readonly UserManager<PathoIdentityUser> userManager;

    public ClaimService(ILogger<ClaimService> logger,
        AppDbContext dbContext,
        UserManager<PathoIdentityUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

        this.logger = logger;
        this.dbContext = dbContext;
        this.userManager = userManager;
    }

    public async Task<PathoClaim> GetClaimAsync(int id)
    {
        return await dbContext.PathoClaims
            .FirstOrDefaultAsync(c => c.Id == id)
            .ConfigureAwait(false);
    }

    public async IAsyncEnumerable<PathoClaim> GetClaimsAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        var col = dbContext.PathoClaims
            .Where(c => c.UserId == userId)
            .AsAsyncEnumerable();

        await foreach (var item in col.ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<PathoClaim> GetClaimsAsync(string userId, Guid appId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId, nameof(userId));

        var col = dbContext.PathoClaims
            .Where(c => c.UserId == userId && c.ApplicationId == appId)
            .AsAsyncEnumerable();

        await foreach (var item in col.ConfigureAwait(false))
        {
            yield return item;
        }
    }

    public async Task RemoveClaimAsync(int claimId)
    {
        var claim = await dbContext.PathoClaims
            .FirstOrDefaultAsync(c => c.Id == claimId)
            .ConfigureAwait(false);

        if (claim == null)
        {
            return;
        }

        var type = claim.Type;
        var value = claim.Value;
        var userName = claim.User.UserName;
        var appId = claim.ApplicationId;

        dbContext.PathoClaims.Remove(claim);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Added a claim of type {ClaimType} and value {ClaimValue} for user {UserName} and application with ID {AppId}",
            type, value, userName, appId);
    }

    public async Task<int> SetClaimAsync(Guid appId, string userName, string type, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName, nameof(userName));
        ArgumentException.ThrowIfNullOrWhiteSpace(type, nameof(type));
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        var user = await userManager.FindByNameAsync(userName).ConfigureAwait(false);

        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"User {userName} doesn't exist.");
        }

        var appExists = await dbContext.Applications
            .AnyAsync(a => a.Id == appId)
            .ConfigureAwait(false);

        if (!appExists)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"Application with ID {appId} doesn't exist.");
        }

        var claim = await dbContext.PathoClaims
            .FirstOrDefaultAsync(c => c.ApplicationId == appId && c.UserId == user.Id && c.Type == type && c.Value == value)
            .ConfigureAwait(false);

        if (claim != null)
        {
            return claim.Id;
        }

        claim = new PathoClaim
        {
            ApplicationId = appId,
            UserId = user.Id,
            Type = type,
            Value = value
        };

        dbContext.PathoClaims.Add(claim);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Added a claim of type {ClaimType} and value {ClaimValue} for user {UserName} and application with ID {AppId}",
            type, value, userName, appId);
        return claim.Id;
    }
}
