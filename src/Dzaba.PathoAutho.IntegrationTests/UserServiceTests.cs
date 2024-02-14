using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class UserServiceTests : IocTestFixture
{
    private IUserService CreateSut()
    {
        return Container.GetRequiredService<IUserService>();
    }

    [Test]
    public async Task RegisterAsync_WhenModel_ThenNewUser()
    {
        var model = new RegisterUser
        {
            Email = "test@test.com",
            Password = "Password1!"
        };

        var sut = CreateSut();
        await sut.RegisterAsync(model).ConfigureAwait(false);

        var user = await sut.FindUserByNameAsync(model.Email).ConfigureAwait(false);
        user.Should().NotBeNull();
    }
}
