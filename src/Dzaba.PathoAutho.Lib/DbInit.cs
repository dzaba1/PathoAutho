using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib;

public interface IDbInit
{
    Task InitAsync();
}

internal sealed class DbInit : IDbInit
{
    private readonly AppDbContext dbContext;
    private readonly RoleManager<IdentityRole> roleManager;

    public DbInit(AppDbContext dbContext,
        RoleManager<IdentityRole> roleManager)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(roleManager, nameof(roleManager));

        this.dbContext = dbContext;
        this.roleManager = roleManager;
    }

    public async Task InitAsync()
    {
        await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);

        var identityResult = await roleManager.CreateAsync(new IdentityRole(RoleNames.SuperAdmin))
            .ConfigureAwait(false);
        identityResult.EnsureSuccess();
    }
}
