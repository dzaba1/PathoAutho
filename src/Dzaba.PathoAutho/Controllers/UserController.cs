using Dzaba.PathoAutho.ActionFilters;
using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Dzaba.PathoAutho.Controllers;

[ApiController]
[Route("[controller]")]
[HandleErrors]
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpPost]
    [ValidateModel]
    public async Task Register([FromBody, Required] RegisterUser newUser)
    {
        await userService.RegisterAsync(newUser).ConfigureAwait(false);
    }

    [HttpGet("current")]
    public async Task<UserWithPermissions> GetCurrent()
    {
        var user = await userService.FindUserByNameAsync(User.Identity.Name).ConfigureAwait(false);
        return new UserWithPermissions
        {
            User = new User
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email,
                Name = user.UserName
            },
            Permisions = user.Permissions.Select(p => new Permision
            {
                Id = p.PermissionId,
                Name = p.Permission.Name,
                Application = new Application
                {
                    Id = p.Permission.ApplicationId,
                    Name = p.Permission.Application.Name
                }
            }).ToArray()
        };
    }
}
