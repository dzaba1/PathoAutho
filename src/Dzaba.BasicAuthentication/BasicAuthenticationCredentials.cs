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

    /// <summary>
    /// Tries to parse Authorization header value.
    /// </summary>
    /// <param name="header">Authorization header value.</param>
    /// <param name="result">New <see cref="BasicAuthenticationCredentials"/> object on success. Null on failure.</param>
    /// <returns>True on success.</returns>
    public static bool TryParseHeader(string header, out BasicAuthenticationCredentials result)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            result = null;
            return false;
        }

        if (!AuthenticationHeaderValue.TryParse(header, out var authorizationHeader))
        {
            result = null;
            return false;
        }

        if (!string.Equals(authorizationHeader.Scheme, "Basic", StringComparison.OrdinalIgnoreCase))
        {
            result = null;
            return false;
        }

        var credentialBytes = Convert.FromBase64String(authorizationHeader.Parameter);
        var credentialsArray = Encoding.UTF8.GetString(credentialBytes).Split(':');
        if (credentialsArray.Length >= 2 )
        {
            result = new BasicAuthenticationCredentials(credentialsArray[0], credentialsArray[1]);
            return true;
        }

        result = null;
        return false;
    }
}
