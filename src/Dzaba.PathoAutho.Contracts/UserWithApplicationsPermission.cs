using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class UserWithApplicationsPermission
{
    public User User { get; set; }

    public ApplicationPermissions[] Permissions { get; set; }

    public bool IsSuperAdmin { get; set; }
}

public sealed class ApplicationPermissions
{
    public NamedEntity<Guid> Application { get; set; }

    public bool IsAdmin { get; set; }

    public Role[] Roles { get; set; }

    public Claim[] Claims { get; set; }
}
