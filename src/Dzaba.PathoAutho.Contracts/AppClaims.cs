namespace Dzaba.PathoAutho.Contracts;

public sealed class AppClaims
{
    public Application Application { get; set; }

    public Claim[] Roles { get; set; }

    public Claim[] Permissions { get; set; }
}
