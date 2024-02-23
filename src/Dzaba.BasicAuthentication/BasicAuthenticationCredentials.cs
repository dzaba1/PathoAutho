namespace Dzaba.BasicAuthentication;

/// <summary>
/// User name and password pair
/// </summary>
public sealed class BasicAuthenticationCredentials
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="userName">User name</param>
    /// <param name="password">Password</param>
    public BasicAuthenticationCredentials(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; }
}
