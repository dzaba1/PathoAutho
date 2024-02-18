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
    private readonly IUserService userService;
    private readonly IClaimsService claimsService;

    public DbInit(AppDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        IUserService userService,
        IClaimsService claimsService)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(roleManager, nameof(roleManager));
        ArgumentNullException.ThrowIfNull(userService, nameof(userService));
        ArgumentNullException.ThrowIfNull(claimsService, nameof(claimsService));

        this.dbContext = dbContext;
        this.roleManager = roleManager;
        this.userService = userService;
        this.claimsService = claimsService;
    }

    public async Task InitAsync()
    {
        await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);

        var identityResult = await roleManager.CreateAsync(new IdentityRole(RoleNames.SuperAdmin))
            .ConfigureAwait(false);
        identityResult.EnsureSuccess();

        var user = await userService.RegisterAsync(new Contracts.RegisterUser
        {
            Email = "admin@test.com",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        }).ConfigureAwait(false);

        await claimsService.AssignUserToIdentiyRoleAsync(user, RoleNames.SuperAdmin)
            .ConfigureAwait(false);
    }
}
