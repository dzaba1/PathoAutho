namespace Dzaba.PathoAutho.Contracts;

public sealed class UserClaims
{
    public User User { get; set; }

    public AppClaims[] Claims { get; set; }
}
