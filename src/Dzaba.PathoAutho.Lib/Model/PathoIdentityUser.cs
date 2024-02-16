using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib.Model;

public class PathoIdentityUser : IdentityUser
{
    public PathoIdentityUser(string userName)
        : base(userName)
    {
    }

    public virtual ICollection<UserPermission> Permissions { get; set; }

    public virtual ICollection<PathoUserRole> PathoRoles { get; set; }
}
