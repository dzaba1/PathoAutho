using Dzaba.PathoAutho.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Dzaba.PathoAutho.Lib.Model;

public class PathoIdentityUser : IdentityUser
{
    public PathoIdentityUser(string userName)
        : base(userName)
    {
    }

    public virtual ICollection<PathoClaim> PathoClaims { get; set; }

    public virtual ICollection<PathoUserRole> PathoRoles { get; set; }

    public virtual ICollection<ApplicationAdmin> AdminApplications { get; set; }

    public User ToModel()
    {
        return new User
        {
            Id = Guid.Parse(Id),
            Email = Email,
            Name = UserName
        };
    }
}
