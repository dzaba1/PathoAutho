using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using NUnit.Framework;
using System.Net;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class ClaimsServiceTest : IocTestFixture
{
    [Test]
    public async Task SetClaimAsync_WhenUserDoesntExist_ThenError()
    {
        var appService = GetApplicationService();
        var sut = GetClaimService();

        var app = await appService.NewApplicationAsync("App").ConfigureAwait(false);

        var ex = await this.Awaiting(_ => sut.SetClaimAsync(app, Guid.NewGuid().ToString(), "Test", "Test"))
            .Should().ThrowAsync<HttpResponseException>()
            .ConfigureAwait(false);

        ex.And.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SetClaimAsync_WhenAppDoesntExist_ThenError()
    {
        var user = await AddUserAsync().ConfigureAwait(false);
        var sut = GetClaimService();

        var ex = await this.Awaiting(_ => sut.SetClaimAsync(Guid.NewGuid(), user.UserName, "Test", "Test"))
            .Should().ThrowAsync<HttpResponseException>()
            .ConfigureAwait(false);

        ex.And.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task SetClaimAsync_WhenCalledTwice_ThenOnlyIdIsReturned()
    {
        var user = await AddUserAsync().ConfigureAwait(false);
        var appService = GetApplicationService();
        var sut = GetClaimService();

        var app = await appService.NewApplicationAsync("App").ConfigureAwait(false);

        var id = await sut.SetClaimAsync(app, user.UserName, "Test", "Test").ConfigureAwait(false);
        var claim = await sut.GetClaimAsync(id).ConfigureAwait(false);
        claim.Should().NotBeNull();

        var newId = await sut.SetClaimAsync(app, user.UserName, "Test", "Test").ConfigureAwait(false);
        newId.Should().Be(id);
    }

    [Test]
    public async Task RemoveClaimAsync_WhenCalled_ThenClaimIsDeleted()
    {
        var user = await AddUserAsync().ConfigureAwait(false);
        var appService = GetApplicationService();
        var sut = GetClaimService();

        var app = await appService.NewApplicationAsync("App").ConfigureAwait(false);

        var id = await sut.SetClaimAsync(app, user.UserName, "Test", "Test").ConfigureAwait(false);

        await sut.RemoveClaimAsync(id).ConfigureAwait(false);
        var claim = await sut.GetClaimAsync(id).ConfigureAwait(false);
        claim.Should().BeNull();
    }
}
