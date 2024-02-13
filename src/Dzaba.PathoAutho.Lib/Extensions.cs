using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib;

public static class Extensions
{
    public static void EnsureSuccess(this IdentityResult result)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));

        if (result.Succeeded)
        {
            return;
        }

        throw new ModelStateException(result.ToString(), result.Errors.Select(e => new KeyValuePair<string, string>(e.Code, e.Description))); 
    }
}
