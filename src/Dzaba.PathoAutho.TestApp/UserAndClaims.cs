namespace Dzaba.PathoAutho.TestApp;

public sealed class UserAndClaims
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public ClaimModel[] Claims { get; set; }
}

public sealed class ClaimModel
{
    public string Type { get; set; }
    public string Value { get; set; }
    public string ValueType { get; set; }
}