using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using NUnit.Framework;
using System.Net;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class PermissionServiceTests : IocTestFixture
{
    [Test]
    public async Task NewPermissionAsync_WhenTheSameButDifferentApp_ThenItWorks()
    {
        var appService = GetApplicationService();
        var sut = GetPermisionService();

        var app1 = await appService.NewApplicationAsync("App1").ConfigureAwait(false);
        var app2 = await appService.NewApplicationAsync("App2").ConfigureAwait(false);

        await sut.NewPermissionAsync(app1, "Permission").ConfigureAwait(false);
        await sut.NewPermissionAsync(app2, "Permission").ConfigureAwait(false);
    }

    [Test]
    public async Task NewPermissionAsync_WhenTheSameAndSameApp_ThenError()
    {
        var appService = GetApplicationService();
        var sut = GetPermisionService();

        var app1 = await appService.NewApplicationAsync("App1").ConfigureAwait(false);

        await sut.NewPermissionAsync(app1, "Permission").ConfigureAwait(false);
        var ex = await this.Invoking(_ => sut.NewPermissionAsync(app1, "Permission"))
            .Should().ThrowAsync<HttpResponseException>()
            .ConfigureAwait(false);
        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AssignUserToPermissionAsync_WhenCalledTwice_ThenNothingHappens()
    {
        var appService = GetApplicationService();
        var sut = GetPermisionService();

        var app = await appService.NewApplicationAsync("App").ConfigureAwait(false);
        var user = await AddUserAsync().ConfigureAwait(false);
        var permission = await sut.NewPermissionAsync(app, "Permission").ConfigureAwait(false);

        await sut.AssignUserToPermissionAsync(user.Id, permission).ConfigureAwait(false);
        await sut.AssignUserToPermissionAsync(user.Id, permission).ConfigureAwait(false);
    }

    [Test]
    public async Task RemovePermissionAsync_WhenPermissionExists_ThenItIsRemoved()
    {
        var appService = GetApplicationService();
        var sut = GetPermisionService();

        var app = await appService.NewApplicationAsync("App1").ConfigureAwait(false);
        var permissionId = await sut.NewPermissionAsync(app, "Permission").ConfigureAwait(false);
        var permission = await sut.GetPermissionAsync(permissionId).ConfigureAwait(false);
        permission.Should().NotBeNull();

        await sut.RemovePermissionAsync(permissionId).ConfigureAwait(false);

        permission = await sut.GetPermissionAsync(permissionId).ConfigureAwait(false);
        permission.Should().BeNull();
    }

    [Test]
    public async Task RemoveUserFromRoleAsync_WhenUserAssigned_ThenItsNotAssigned()
    {
        var appService = GetApplicationService();
        var sut = GetPermisionService();

        var app = await appService.NewApplicationAsync("App").ConfigureAwait(false);
        var user = await AddUserAsync().ConfigureAwait(false);
        var permission = await sut.NewPermissionAsync(app, "Permission").ConfigureAwait(false);

        await sut.AssignUserToPermissionAsync(user.Id, permission).ConfigureAwait(false);

        var userPermissions = await sut.GetPermissionsAsync(user.Id)
            .ToArrayAsync()
            .ConfigureAwait(false);

        userPermissions.Should().HaveCount(1);

        await sut.RemoveUserFromPermissionAsync(user.Id, permission).ConfigureAwait(false);

        userPermissions = await sut.GetPermissionsAsync(user.Id)
            .ToArrayAsync()
            .ConfigureAwait(false);

        userPermissions.Should().BeEmpty();
    }
}
