using System;

namespace Dzaba.PathoAutho.Contracts;

public sealed class Claim
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public Guid ApplicationId { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
}
