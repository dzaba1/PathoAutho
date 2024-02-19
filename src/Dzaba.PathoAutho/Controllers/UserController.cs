using Dzaba.PathoAutho.ActionFilters;
using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public async Task Register([FromBody, Required] RegisterUser newUser)
    {
        await userService.RegisterAsync(newUser).ConfigureAwait(false);
    }
}
