using AutoFixture;
using Dzaba.BasicAuthentication;
using Dzaba.PathoAutho.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib.Tests;

[TestFixture]
public class BasicAuthenticationHandlerServiceTests
{
    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private BasicAuthenticationHandlerService CreateSut()
    {
        return fixture.Create<BasicAuthenticationHandlerService>();
    }

    private BasicAuthenticationCredentials CreateCredentials()
    {
        return new BasicAuthenticationCredentials("user", "Password1!");
    }

    [Test]
    public async Task CheckPasswordAsync_WhenUserDoesntExist_ThenFail()
    {
        var creds = CreateCredentials();

        fixture.FreezeMock<IUserService>()
            .Setup(x => x.FindUserByNameAsync(creds.UserName))
            .ReturnsAsync((PathoIdentityUser)null);

        var sut = CreateSut();

        var result = await sut.CheckPasswordAsync(creds, null).ConfigureAwait(false);
        result.Success.Should().BeFalse();
        result.Context.Should().BeNull();
    }

    [Test]
    public async Task CheckPasswordAsync_WhenUserFoundButWrongPassword_ThenFailWithContext()
    {
        var creds = CreateCredentials();
        var user = new PathoIdentityUser(creds.UserName);

        fixture.FreezeMock<IUserService>()
            .Setup(x => x.FindUserByNameAsync(creds.UserName))
            .ReturnsAsync(user);

        fixture.FreezeMock<ILoginService>()
            .Setup(x => x.PasswordMatchAsync(user, creds.Password))
            .ReturnsAsync(false);

        var sut = CreateSut();

        var result = await sut.CheckPasswordAsync(creds, null).ConfigureAwait(false);
        result.Success.Should().BeFalse();
        result.Context.Should().Be(user);
    }

    [Test]
    public async Task CheckPasswordAsync_WhenUserFoundAndCorrectPassword_ThenSuccess()
    {
        var creds = CreateCredentials();
        var user = new PathoIdentityUser(creds.UserName);

        fixture.FreezeMock<IUserService>()
            .Setup(x => x.FindUserByNameAsync(creds.UserName))
            .ReturnsAsync(user);

        fixture.FreezeMock<ILoginService>()
            .Setup(x => x.PasswordMatchAsync(user, creds.Password))
            .ReturnsAsync(true);

        var sut = CreateSut();

        var result = await sut.CheckPasswordAsync(creds, null).ConfigureAwait(false);
        result.Success.Should().BeTrue();
        result.Context.Should().Be(user);
    }

    [Test]
    public async Task AddClaimsAsync_WhenCalled_ThenIdWithEmailsWithRolesAreSet()
    {
        var creds = CreateCredentials();
        var user = new PathoIdentityUser(creds.UserName)
        {
            Email = "test@test.com"
        };
        var claims = new List<Claim>();

        fixture.FreezeMock<IRoleService>()
            .Setup(x => x.GetIdentityRolesAsync(user))
            .Returns(new[] { "SuperUser" }.ToAsyncEnumerable());

        var sut = CreateSut();

        await sut.AddClaimsAsync(creds, null, claims, user).ConfigureAwait(false);

        claims.Should().HaveCount(3);
        claims[0].Value.Should().Be(user.Id);
        claims[1].Value.Should().Be(user.Email);
        claims[1].Type.Should().Be(ClaimTypes.Email);
        claims[2].Value.Should().Be("SuperUser");
        claims[2].Type.Should().Be(ClaimTypes.Role);
    }
}
