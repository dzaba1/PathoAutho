using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class ApplicationData
{
    public NamedEntity<Guid> Application { get; set; }
    public User[] Admins { get; set; }
    public Role[] Roles { get; set; }
    public Claim[] Claims { get; set; }
    public UserRoleAssingment[] RoleAssingments { get; set; }
}
