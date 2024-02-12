using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib
{
    public static class Extensions
    {
        public static void EnsureSuccess(this IdentityResult result)
        {
            ArgumentNullException.ThrowIfNull(result, nameof(result));

            if (result.Succeeded)
            {
                return;
            }

            throw new IdentityException(result.ToString(), result.Errors); 
        }
    }
}
