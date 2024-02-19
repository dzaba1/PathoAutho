using Dzaba.PathoAutho.ActionFilters;
using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Dzaba.PathoAutho.Controllers;

[ApiController]
[Route("[controller]")]
[HandleErrors]
public class RoleController : ControllerBase
{
    private readonly IRoleService roleService;
    private readonly IUserService userService;
    private readonly IAuthHelper authHelper;

    public RoleController(IRoleService roleService,
        IUserService userService,
        IAuthHelper authHelper)
    {
        this.roleService = roleService;
        this.userService = userService;
        this.authHelper = authHelper;
    }

    [HttpPost("identity/{role}/user/{userName}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task AssignUserToIdentiyRoleAsync(string userName, string role)
    {
        var user = await userService.FindUserByNameAsync(userName)
            .ConfigureAwait(false);

        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"User {userName} not found.");
        }

        await roleService.AssignUserToIdentiyRoleAsync(user, role)
            .ConfigureAwait(false);
    }

    [HttpDelete("identity/{role}/user/{userName}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task RemoveUserFromIdentiyRoleAsync(string userName, string role)
    {
        var user = await userService.FindUserByNameAsync(userName)
            .ConfigureAwait(false);

        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"User {userName} not found.");
        }

        await roleService.RemoveUserFromIdentiyRoleAsync(user, role)
            .ConfigureAwait(false);
    }

    [HttpPost]
    [Authorize]
    [ValidateModel]
    public async Task<int> NewRole([Required, FromBody] NewRole newRole)
    {
        await authHelper.CheckSuperOrAppAdminAsync(User, newRole.ApplicationId)
            .ConfigureAwait(false);

        return await roleService.NewRoleAsync(newRole.ApplicationId, newRole.RoleName)
            .ConfigureAwait(false);
    }

    [HttpPost("{roleId}/user/{userName}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task AssignUserToRoleAsync(string userName, int roleId)
    {
        var user = await userService.FindUserByNameAsync(userName)
            .ConfigureAwait(false);

        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"User {userName} not found.");
        }

        await roleService.AssignUserToRoleAsync(user.Id, roleId)
            .ConfigureAwait(false);
    }

    [HttpDelete("{roleId}/user/{userName}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task RemoveUserFromRoleAsync(string userName, int roleId)
    {
        var user = await userService.FindUserByNameAsync(userName)
            .ConfigureAwait(false);

        if (user == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"User {userName} not found.");
        }

        await roleService.RemoveUserFromRoleAsync(user.Id, roleId)
            .ConfigureAwait(false);
    }
}
