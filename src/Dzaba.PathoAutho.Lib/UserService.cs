using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib;

public interface IUserService
{
    Task RegisterAsync(RegisterUser newUser);
}

internal sealed class UserService : IUserService
{
    private readonly UserManager<PathoIdentityUser> userManager;

    public UserService(UserManager<PathoIdentityUser> userManager)
    {
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

        this.userManager = userManager;
    }

    public async Task RegisterAsync(RegisterUser newUser)
    {
        ArgumentNullException.ThrowIfNull(newUser, nameof(newUser));

        var identity = new PathoIdentityUser(newUser.Email)
        {
            Email = newUser.Email
        };

        var result = await userManager.CreateAsync(identity, newUser.Password)
            .ConfigureAwait(false);

        result.EnsureSuccess();
    }
}
