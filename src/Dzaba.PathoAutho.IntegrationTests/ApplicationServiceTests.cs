using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using NUnit.Framework;
using System.Net;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class ApplicationServiceTests : IocTestFixture
{
    [Test]
    public async Task NewApplicationAsync_WhenTheAppExists_ThenException()
    {
        var sut = GetApplicationService();

        await sut.NewApplicationAsync("App1").ConfigureAwait(false);

        var ex = await this.Awaiting(_ => sut.NewApplicationAsync("App1"))
            .Should().ThrowAsync<HttpResponseException>()
            .ConfigureAwait(false);
        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ChangeNameAsync_WhenTheAppExists_ThenException()
    {
        var sut = GetApplicationService();

        var id = await sut.NewApplicationAsync("App1").ConfigureAwait(false);
        await sut.NewApplicationAsync("App2").ConfigureAwait(false);

        var ex = await this.Awaiting(_ => sut.ChangeNameAsync(id, "App2"))
            .Should().ThrowAsync<HttpResponseException>()
            .ConfigureAwait(false);
        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ChangeNameAsync_WhenNewName_ThenNewName()
    {
        var sut = GetApplicationService();

        var id = await sut.NewApplicationAsync("App1").ConfigureAwait(false);

        await sut.ChangeNameAsync(id, "App2").ConfigureAwait(false);

        var app = await sut.GetApplicationAsync(id).ConfigureAwait(false);
        app.Name.Should().Be("App2");
    }

    [Test]
    public async Task RemoveApplicationAsync_WhenAppExists_ThenItIsDeleted()
    {
        var sut = GetApplicationService();

        var id = await sut.NewApplicationAsync("App1").ConfigureAwait(false);
        var app = await sut.GetApplicationAsync(id).ConfigureAwait(false);

        app.Should().NotBeNull();

        await sut.RemoveApplicationAsync(id).ConfigureAwait(false);

        app = await sut.GetApplicationAsync(id).ConfigureAwait(false);
        app.Should().BeNull();
    }
}
