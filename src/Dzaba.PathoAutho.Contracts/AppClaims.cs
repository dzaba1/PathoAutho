namespace Dzaba.PathoAutho.Contracts;

public sealed class AppClaims
{
    public Application Application { get; set; }

    public ClaimBase[] Roles { get; set; }

    public ClaimBase[] Permissions { get; set; }
}
