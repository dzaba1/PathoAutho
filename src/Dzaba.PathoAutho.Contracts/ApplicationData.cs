using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class ApplicationData
{
    public NamedEntity<Guid> Application { get; set; }

    public User[] Admins { get; set; }

    public ClaimAssignment[] Roles { get; set; }

    public ClaimAssignment[] Permissions { get; set; }
}
