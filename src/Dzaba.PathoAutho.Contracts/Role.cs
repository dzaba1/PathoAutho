using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class Role : NamedEntity<int>
{
    public Guid ApplicationId { get; set; }
}
