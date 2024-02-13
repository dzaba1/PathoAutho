using Dzaba.PathoAutho.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib;

public interface IUserService
{
    Task RegisterAsync(RegisterUser newUser);
}

internal sealed class UserService : IUserService
{
    private readonly UserManager<IdentityUser> userManager;

    public UserService(UserManager<IdentityUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

        this.userManager = userManager;
    }

    public async Task RegisterAsync(RegisterUser newUser)
    {
        ArgumentNullException.ThrowIfNull(newUser, nameof(newUser));

        var identity = new IdentityUser(newUser.Email)
        {
            Email = newUser.Email
        };

        var result = await userManager.CreateAsync(identity, newUser.Password)
            .ConfigureAwait(false);

        result.EnsureSuccess();
    }
}
