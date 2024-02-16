﻿using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class ClaimsServiceTest : IocTestFixture
{
    private IClaimsService CreateSut()
    {
        return Container.GetRequiredService<IClaimsService>();
    }

    [Test]
    public async Task GetAppClaimsModelForUserAsync_WhenApplicationAndClaims_ThenCorrectModel()
    {
        var user = await AddUserAsync().ConfigureAwait(false);

        var sut = CreateSut();

        var app1Id = await sut.NewApplicationAsync("App1").ConfigureAwait(false);
        var app2Id = await sut.NewApplicationAsync("App2").ConfigureAwait(false);

        var role1Id = await sut.NewRoleAsync(app1Id, "Role1").ConfigureAwait(false);
        await sut.NewRoleAsync(app2Id, "Role1").ConfigureAwait(false);
        var role2Id = await sut.NewRoleAsync(app1Id, "Role2").ConfigureAwait(false);
        await sut.NewRoleAsync(app2Id, "Role2").ConfigureAwait(false);

        var permission1Id = await sut.NewPermissionAsync(app1Id, "Permission1").ConfigureAwait(false);
        await sut.NewPermissionAsync(app2Id, "Permission1").ConfigureAwait(false);
        var permission2Id = await sut.NewPermissionAsync(app1Id, "Permission2").ConfigureAwait(false);
        await sut.NewPermissionAsync(app2Id, "Permission2").ConfigureAwait(false);

        await sut.AssignUserToRoleAsync(user.Id, role1Id).ConfigureAwait(false);
        await sut.AssignUserToRoleAsync(user.Id, role2Id).ConfigureAwait(false);

        await sut.AssignUserToPermissionAsync(user.Id, permission1Id).ConfigureAwait(false);
        await sut.AssignUserToPermissionAsync(user.Id, permission2Id).ConfigureAwait(false);

        var model = await sut.GetAppClaimsModelForUserAsync(user.Id)
            .ToArrayAsync()
            .ConfigureAwait(false);

        model.Should().HaveCount(1);
        model[0].Application.Id.Should().Be(app1Id);
        model[0].Application.Name.Should().Be("App1");
        model[0].Roles.Should().HaveCount(2);
        model[0].Permissions.Should().HaveCount(2);
        model[0].Roles[0].Id.Should().Be(role1Id);
        model[0].Roles[0].Name.Should().Be("Role1");
        model[0].Roles[1].Id.Should().Be(role2Id);
        model[0].Roles[1].Name.Should().Be("Role2");
        model[0].Permissions[0].Id.Should().Be(permission1Id);
        model[0].Permissions[0].Name.Should().Be("Permission1");
        model[0].Permissions[1].Id.Should().Be(permission2Id);
        model[0].Permissions[1].Name.Should().Be("Permission2");
    }
}