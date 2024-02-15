using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib;

public interface ILoginService
{
    Task<bool> PasswordMatchAsync(PathoIdentityUser user, string password);
}

internal sealed class LoginService : ILoginService
{
    private readonly SignInManager<PathoIdentityUser> signInManager;

    public LoginService(SignInManager<PathoIdentityUser> signInManager)
    {
        ArgumentNullException.ThrowIfNull(signInManager, nameof(signInManager));

        this.signInManager = signInManager;
    }

    public async Task<bool> PasswordMatchAsync(PathoIdentityUser user, string password)
    {
        var result = await signInManager.CheckPasswordSignInAsync(user, password, false)
            .ConfigureAwait(false);

        return result.Succeeded;
    }
}
