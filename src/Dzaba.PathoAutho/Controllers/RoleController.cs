using Dzaba.PathoAutho.ActionFilters;
using Dzaba.PathoAutho.Lib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Dzaba.PathoAutho.Controllers;

[ApiController]
[Route("[controller]")]
[HandleErrors]
public class RoleController : ControllerBase
{
    private readonly IRoleService roleService;
    private readonly IUserService userService;

    public RoleController(IRoleService roleService,
        IUserService userService)
    {
        this.roleService = roleService;
        this.userService = userService;
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
}
