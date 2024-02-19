using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class User : NamedEntity<Guid>
{
    public string Email { get; set; }
}
