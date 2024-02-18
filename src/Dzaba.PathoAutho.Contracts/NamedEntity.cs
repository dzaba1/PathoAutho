using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class NamedEntity<TId>
    where TId : IEquatable<TId>
{
    public TId Id { get; set; }
    public string Name { get; set; }
}
