using Dzaba.PathoAutho.Lib.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Dzaba.PathoAutho.Lib;

public interface IApplicationService
{
    Task<Guid> NewApplicationAsync(string appName);
    Task RemoveApplicationAsync(Guid id);
    Task ChangeNameAsync(Guid id, string newName);
    Task<Application> GetApplicationAsync(Guid id);
}

internal sealed class ApplicationService : IApplicationService
{
    private readonly AppDbContext dbContext;
    private readonly ILogger<ApplicationService> logger;

    public ApplicationService(AppDbContext dbContext,
        ILogger<ApplicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task ChangeNameAsync(Guid id, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));

        var app = await dbContext.Applications.FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);

        if (app == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"Application with ID {id} doesn't exist.");
        }

        var exist = await dbContext.Applications.AnyAsync(p => p.Name == newName)
            .ConfigureAwait(false);

        if (exist)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"Application {newName} already exists.");
        }

        var oldName = app.Name;
        app.Name = newName;
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Changed application name {OldApplicationName} to {NewApplicationName}", oldName, newName);
    }

    public async Task<Application> GetApplicationAsync(Guid id)
    {
        return await dbContext.Applications.FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<Guid> NewApplicationAsync(string appName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName, nameof(appName));

        var exist = await dbContext.Applications.AnyAsync(p => p.Name == appName)
            .ConfigureAwait(false);

        if (exist)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"Application {appName} already exists.");
        }

        var entity = new Application
        {
            Name = appName
        };

        dbContext.Applications.Add(entity);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Created a new application {AppName}", appName);

        return entity.Id;
    }

    public async Task RemoveApplicationAsync(Guid id)
    {
        var app = await dbContext.Applications.FirstOrDefaultAsync(p => p.Id == id)
            .ConfigureAwait(false);

        if (app == null)
        {
            return;
        }

        dbContext.Applications.Remove(app);
        await dbContext.SaveChangesAsync().ConfigureAwait(false);

        logger.LogInformation("Removed application {AppName}", app.Name);
    }
}
