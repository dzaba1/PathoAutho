using AutoFixture;
using Dzaba.PathoAutho.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Security.Claims;

namespace Dzaba.PathoAutho.Lib.Tests;

[TestFixture]
public class HttpClaimsServiceTests
{
    private IFixture fixture;

    [SetUp]
    public void Setup()
    {
        fixture = TestFixture.Create();
    }

    private HttpClaimsService CreateSut()
    {
        return fixture.Create<HttpClaimsService>();
    }

    private HttpContext GetHttpContext()
    {
        return Mock.Of<HttpContext>();
    }

    [Test]
    public async Task GetAppClaimsAsync_WhenUserIsSuperAdmin_ThenClaim()
    {
        var user = new PathoIdentityUser("user");

        fixture.FreezeMock<IRoleService>()
            .Setup(x => x.GetIdentityRolesAsync(user))
            .Returns(new[] { RoleNames.SuperAdmin }.ToAsyncEnumerable());

        var sut = CreateSut();

        var claims = await sut.GetAppClaimsAsync(user, GetHttpContext()).ToArrayAsync()
            .ConfigureAwait(false);

        claims.Should().HaveCount(1);
        claims[0].Value.Should().Be(RoleNames.SuperAdmin);
        claims[0].Type.Should().Be(ClaimTypes.Role);
    }
}
