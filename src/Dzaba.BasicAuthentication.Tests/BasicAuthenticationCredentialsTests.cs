using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.BasicAuthentication.Tests;

[TestFixture]
public class BasicAuthenticationCredentialsTests
{
    [Test]
    public void ToHeaderValue_WhenEncoded_ThenItCanBeEncoded()
    {
        var creds = new BasicAuthenticationCredentials("Username", "Password1!");

        var header = creds.ToHeaderValue();
        var result = BasicAuthenticationCredentials.TryParseHeader(header.ToString(), out var resultCreds);
        result.Should().BeTrue();

        resultCreds.Should().BeEquivalentTo(creds);
    }
}
