using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class LoginServiceTests : IocTestFixture
{
    private ILoginService CreateSut()
    {
        return Container.GetRequiredService<ILoginService>();
    }

    [Test]
    public async Task PasswordMatchAsync_WhenUserCreated_ThenPasswordMatch()
    {
        var user = await AddUserAsync().ConfigureAwait(false);

        var sut = CreateSut();

        var result = await sut.PasswordMatchAsync(user, "Password1!").ConfigureAwait(false);
        result.Should().BeTrue();
    }
}
