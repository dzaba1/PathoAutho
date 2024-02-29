using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dzaba.PathoAutho.Lib;

public interface IUserService
{
    Task<PathoIdentityUser> RegisterAsync(RegisterUser newUser);
    Task<PathoIdentityUser> FindUserByNameAsync(string userName);
    Task DeleteUserAsync(string userName);
}

internal sealed class UserService : IUserService
{
    private readonly UserManager<PathoIdentityUser> userManager;
    private readonly ILogger<UserService> logger;

    public UserService(UserManager<PathoIdentityUser> userManager,
        ILogger<UserService> logger)
    {
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        this.userManager = userManager;
        this.logger = logger;
    }

    public async Task DeleteUserAsync(string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName, nameof(userName));

        var user = await FindUserByNameAsync(userName).ConfigureAwait(false);
        if (user == null)
        {
            return;
        }

        var deleteResult = await userManager.DeleteAsync(user)
            .ConfigureAwait(false);
        deleteResult.EnsureSuccess();

        logger.LogInformation("Delted user {UserName}", userName);
    }

    public async Task<PathoIdentityUser> FindUserByNameAsync(string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName, nameof(userName));

        logger.LogDebug("Start searching for user by name {UserName}", userName);

        return await userManager.FindByNameAsync(userName).ConfigureAwait(false);
    }

    public async Task<PathoIdentityUser> RegisterAsync(RegisterUser newUser)
    {
        ArgumentNullException.ThrowIfNull(newUser, nameof(newUser));

        logger.LogDebug("Start adding new user by email {Email}", newUser.Email);

        var identity = new PathoIdentityUser(newUser.Email)
        {
            Email = newUser.Email
        };

        var result = await userManager.CreateAsync(identity, newUser.Password)
            .ConfigureAwait(false);

        result.EnsureSuccess();

        logger.LogInformation("Added a new user with email {Email}", newUser.Email);

        return identity;
    }
}
