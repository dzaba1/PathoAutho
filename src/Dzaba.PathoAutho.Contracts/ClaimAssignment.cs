namespace Dzaba.PathoAutho.Contracts;

public sealed class ClaimAssignment
{
    public NamedEntity<int> Claim { get; set; }

    public User[] Users { get; set; }
}
