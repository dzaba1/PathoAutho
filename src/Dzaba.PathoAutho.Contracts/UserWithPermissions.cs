namespace Dzaba.PathoAutho.Contracts;

public sealed class UserWithPermissions
{
    public User User { get; set; }

    public Permision[] Permisions { get; set; }
}
