using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib;
using Dzaba.PathoAutho.Lib.Model;
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

    private async Task<PathoIdentityUser> AddUserAsync()
    {
        var model = new RegisterUser
        {
            Email = "test@test.com",
            Password = "Password1!"
        };

        var userMgr = Container.GetRequiredService<IUserService>();
        await userMgr.RegisterAsync(model).ConfigureAwait(false);

        return await userMgr.FindUserByNameAsync(model.Email).ConfigureAwait(false);
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
