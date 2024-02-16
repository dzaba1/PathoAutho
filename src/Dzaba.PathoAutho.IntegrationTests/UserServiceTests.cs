using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class UserServiceTests : IocTestFixture
{
    [Test]
    public async Task RegisterAsync_WhenModel_ThenNewUser()
    {
        var user = await AddUserAsync().ConfigureAwait(false);
        user.Should().NotBeNull();
    }
}
