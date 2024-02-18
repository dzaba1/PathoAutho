using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using NUnit.Framework;
using System.Net;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class RolesServiceTests : IocTestFixture
{
    [Test]
    public async Task NewRoleAsync_WhenTheSameButDifferentApp_ThenItWorks()
    {
        var appService = GetApplicationService();
        var sut = GetRolesService();

        var app1 = await appService.NewApplicationAsync("App1").ConfigureAwait(false);
        var app2 = await appService.NewApplicationAsync("App2").ConfigureAwait(false);

        await sut.NewRoleAsync(app1, "Role").ConfigureAwait(false);
        await sut.NewRoleAsync(app2, "Role").ConfigureAwait(false);
    }

    [Test]
    public async Task NewRoleAsync_WhenTheSameAndSameApp_ThenError()
    {
        var appService = GetApplicationService();
        var sut = GetRolesService();

        var app1 = await appService.NewApplicationAsync("App1").ConfigureAwait(false);

        await sut.NewRoleAsync(app1, "Role").ConfigureAwait(false);
        var ex = await this.Invoking(_ => sut.NewRoleAsync(app1, "Role"))
            .Should().ThrowAsync<HttpResponseException>()
            .ConfigureAwait(false);
        ex.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task AssignUserToRoleAsync_WhenCalledTwice_ThenNothingHappens()
    {
        var appService = GetApplicationService();
        var sut = GetRolesService();

        var app = await appService.NewApplicationAsync("App").ConfigureAwait(false);
        var user = await AddUserAsync().ConfigureAwait(false);
        var role = await sut.NewRoleAsync(app, "Role").ConfigureAwait(false);

        await sut.AssignUserToRoleAsync(user.Id, role).ConfigureAwait(false);
        await sut.AssignUserToRoleAsync(user.Id, role).ConfigureAwait(false);
    }

    [Test]
    public async Task RemoveRoleAsync_RoleExists_ThenItIsRemoved()
    {
        var appService = GetApplicationService();
        var sut = GetRolesService();

        var app = await appService.NewApplicationAsync("App1").ConfigureAwait(false);
        var roleId = await sut.NewRoleAsync(app, "Role").ConfigureAwait(false);
        var role = await sut.GetRoleAsync(roleId).ConfigureAwait(false);
        role.Should().NotBeNull();

        await sut.RemoveRoleAsync(roleId).ConfigureAwait(false);

        role = await sut.GetRoleAsync(roleId).ConfigureAwait(false);
        role.Should().BeNull();
    }

    [Test]
    public async Task RemoveUserFromRoleAsync_WhenUserAssigned_ThenItsNotAssigned()
    {
        var appService = GetApplicationService();
        var sut = GetRolesService();

        var app = await appService.NewApplicationAsync("App").ConfigureAwait(false);
        var user = await AddUserAsync().ConfigureAwait(false);
        var role = await sut.NewRoleAsync(app, "Role").ConfigureAwait(false);

        await sut.AssignUserToRoleAsync(user.Id, role).ConfigureAwait(false);

        var userRoles = await sut.GetRolesAsync(user.Id)
            .ToArrayAsync()
            .ConfigureAwait(false);

        userRoles.Should().HaveCount(1);

        await sut.RemoveUserFromRoleAsync(user.Id, role).ConfigureAwait(false);

        userRoles = await sut.GetRolesAsync(user.Id)
            .ToArrayAsync()
            .ConfigureAwait(false);

        userRoles.Should().BeEmpty();
    }

    [Test]
    public async Task RemoveUserFromIdentiyRoleAsync_WhenUserAssigned_ThenItsNotAssigned()
    {
        var appService = GetApplicationService();
        var sut = GetRolesService();

        var user = await AddUserAsync().ConfigureAwait(false);

        await sut.AssignUserToIdentiyRoleAsync(user, RoleNames.SuperAdmin).ConfigureAwait(false);

        var userRoles = await sut.GetIdentityRolesAsync(user)
            .ToArrayAsync()
            .ConfigureAwait(false);

        userRoles.Should().HaveCount(1);

        await sut.RemoveUserFromIdentiyRoleAsync(user, RoleNames.SuperAdmin).ConfigureAwait(false);

        userRoles = await sut.GetIdentityRolesAsync(user)
            .ToArrayAsync()
            .ConfigureAwait(false);

        userRoles.Should().BeEmpty();
    }

    [Test]
    public async Task RemoveUserFromRoleAsync_WhenUserAssignedToOtherApp_ThenNothing()
    {
        var appService = GetApplicationService();
        var sut = GetRolesService();

        var app1 = await appService.NewApplicationAsync("App1").ConfigureAwait(false);
        var app2 = await appService.NewApplicationAsync("App2").ConfigureAwait(false);
        var user = await AddUserAsync().ConfigureAwait(false);
        var role = await sut.NewRoleAsync(app1, "Role").ConfigureAwait(false);
        await sut.NewRoleAsync(app2, "Role").ConfigureAwait(false);

        await sut.AssignUserToRoleAsync(user.Id, role).ConfigureAwait(false);

        var userRoles = await sut.GetRolesAsync(user.Id, app2)
            .ToArrayAsync()
            .ConfigureAwait(false);

        userRoles.Should().BeEmpty();
    }
}
