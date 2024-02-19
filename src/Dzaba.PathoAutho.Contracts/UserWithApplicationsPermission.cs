namespace Dzaba.PathoAutho.Contracts;

public sealed class UserWithApplicationsPermission
{
    public User User { get; set; }

    public ApplicationPermissions[] Permissions { get; set; }

    public bool IsSuperAdmin { get; set; }
}
