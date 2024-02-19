using AutoFixture;
using Dzaba.PathoAutho.Lib.Model;
using Dzaba.TestUtils;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

    private HttpContext GetHttpContext(string requestPath = null,
        IReadOnlyDictionary<string, object> routeValues = null)
    {
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();

        if (requestPath != null)
        {
            var path = new PathString(requestPath);
            request.Setup(x => x.Path)
                .Returns(path);
        }

        if (routeValues != null)
        {
            request.Setup(x => x.RouteValues)
                .Returns(new RouteValueDictionary(routeValues));
        }

        context.Setup(x => x.Request)
            .Returns(request.Object);

        return context.Object;
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

    [Test]
    public async Task GetAppClaimsAsync_WhenAppIdProvided_ThenAdminRole()
    {
        var appId = Guid.NewGuid();
        var user = new PathoIdentityUser("user");

        fixture.FreezeMock<IRoleService>()
            .Setup(x => x.IsApplicationAdminAsync(user.Id, appId))
            .ReturnsAsync(true);

        var httpContext = GetHttpContext($"/Application/{appId}/",
            new Dictionary<string, object>()
            {
                { "appId", appId.ToString() }
            });

        var sut = CreateSut();

        var claims = await sut.GetAppClaimsAsync(user, httpContext).ToArrayAsync()
            .ConfigureAwait(false);

        claims.Should().HaveCount(1);
        claims[0].Value.Should().Be(RoleNames.AppAdmin);
        claims[0].Type.Should().Be(ClaimTypes.Role);
    }

    [Test]
    public async Task GetAppClaimsAsync_WhenRoleIdProvided_ThenAdminRole()
    {
        var appId = Guid.NewGuid();
        var roleId = 123;
        var user = new PathoIdentityUser("user");

        var roleService = fixture.FreezeMock<IRoleService>();

        roleService.Setup(x => x.IsApplicationAdminAsync(user.Id, appId))
            .ReturnsAsync(true);

        roleService.Setup(x => x.GetRoleAsync(roleId))
            .ReturnsAsync(new PathoRole
            {
                Id = roleId,
                ApplicationId = appId
            });

        var httpContext = GetHttpContext($"/Role/{roleId}/",
            new Dictionary<string, object>()
            {
                { "roleId", roleId.ToString() }
            });

        var sut = CreateSut();

        var claims = await sut.GetAppClaimsAsync(user, httpContext).ToArrayAsync()
            .ConfigureAwait(false);

        claims.Should().HaveCount(1);
        claims[0].Value.Should().Be(RoleNames.AppAdmin);
        claims[0].Type.Should().Be(ClaimTypes.Role);
    }

    [Test]
    public async Task GetAppClaimsAsync_WhenClaimIdProvided_ThenAdminRole()
    {
        var appId = Guid.NewGuid();
        var claimId = 123;
        var user = new PathoIdentityUser("user");

        fixture.FreezeMock<IRoleService>()
            .Setup(x => x.IsApplicationAdminAsync(user.Id, appId))
            .ReturnsAsync(true);

        fixture.FreezeMock<IClaimService>()
            .Setup(x => x.GetClaimAsync(claimId))
            .ReturnsAsync(new PathoClaim
            {
                ApplicationId = appId,
                Id = claimId
            });

        var httpContext = GetHttpContext($"/Claim/{claimId}/",
            new Dictionary<string, object>()
            {
                { "claimId", claimId.ToString() }
            });

        var sut = CreateSut();

        var claims = await sut.GetAppClaimsAsync(user, httpContext).ToArrayAsync()
            .ConfigureAwait(false);

        claims.Should().HaveCount(1);
        claims[0].Value.Should().Be(RoleNames.AppAdmin);
        claims[0].Type.Should().Be(ClaimTypes.Role);
    }
}
