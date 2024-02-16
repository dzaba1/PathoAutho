namespace Dzaba.BasicAuthentication;

public sealed class BasicAuthenticationCredentials
{
    public BasicAuthenticationCredentials(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }

    public string UserName { get; }

    public string Password { get; }
}
