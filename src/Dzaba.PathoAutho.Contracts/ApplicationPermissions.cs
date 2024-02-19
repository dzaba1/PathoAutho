using System;

namespace Dzaba.PathoAutho.Contracts;

public class ApplicationPermissions
{
    public NamedEntity<Guid> Application { get; set; }

    public bool IsAdmin { get; set; }

    public Role[] Roles { get; set; }

    public Claim[] Claims { get; set; }
}

public class ApplicationPermissionsWithUser : ApplicationPermissions
{
    public User User { get; set; }
}
