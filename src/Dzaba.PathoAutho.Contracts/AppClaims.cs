using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class AppClaims
{
    public NamedEntity<Guid> Application { get; set; }

    public NamedEntity<int>[] Roles { get; set; }

    public NamedEntity<int>[] Permissions { get; set; }
}
