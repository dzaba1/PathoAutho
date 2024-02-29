using System.Net.Http.Headers;
using System.Text;

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

    /// <summary>
    /// Creates and populates <see cref="AuthenticationHeaderValue"/>.
    /// </summary>
    /// <returns>New <see cref="AuthenticationHeaderValue"/>.</returns>
    public AuthenticationHeaderValue ToHeaderValue()
    {
        var authenticationString = $"{UserName}:{Password}";
        var base64EncodedAuthenticationString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
        return new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
    }
}
