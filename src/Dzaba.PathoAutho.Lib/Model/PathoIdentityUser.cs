using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib.Model;

public class PathoIdentityUser : IdentityUser
{
    public PathoIdentityUser(string userName)
        : base(userName)
    {
    }
}
