using Dzaba.PathoAutho.Contracts;
using Dzaba.Utils;
using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib;

public interface IRegisterService
{
    Task<IdentityResult> RegisterAsync(RegisterUser newUser);
}

internal sealed class RegisterService : IRegisterService
{
    private readonly UserManager<IdentityUser> userManager;

    public RegisterService(UserManager<IdentityUser> userManager)
    {
        Require.NotNull(userManager, nameof(userManager));

        this.userManager = userManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterUser newUser)
    {
        Require.NotNull(newUser, nameof(newUser));

        var identity = new IdentityUser(newUser.Email)
        {
            Email = newUser.Email
        };

        return await userManager.CreateAsync(identity, newUser.Password)
            .ConfigureAwait(false);
    }
}
