using Dzaba.PathoAutho.Lib;
using FluentAssertions;
using NUnit.Framework;

namespace Dzaba.PathoAutho.IntegrationTests;

[TestFixture]
public class UserServiceTests : IocTestFixture
{
    [Test]
    public async Task RegisterAsync_WhenModel_ThenNewUser()
    {
        var user = await AddUserAsync().ConfigureAwait(false);
        user.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteUserAsync_WhenUserIsSuperAdmin_ThenItCanBeDeleted()
    {
        var sut = GetUserService();
        var rolesService = GetRolesService();

        var user = await AddUserAsync().ConfigureAwait(false);

        await rolesService.AssignUserToIdentiyRoleAsync(user, RoleNames.SuperAdmin)
            .ConfigureAwait(false);

        await sut.DeleteUserAsync(user.UserName).ConfigureAwait(false);

        user = await sut.FindUserByNameAsync(user.UserName).ConfigureAwait(false);
        user.Should().BeNull();
    }
}
