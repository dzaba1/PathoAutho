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
    private readonly IClaimsService claimsService;

    public UserController(IUserService userService,
        IClaimsService claimsService)
    {
        this.userService = userService;
        this.claimsService = claimsService;
    }

    [HttpPost]
    [ValidateModel]
    [AllowAnonymous]
    public async Task Register([FromBody, Required] RegisterUser newUser)
    {
        await userService.RegisterAsync(newUser).ConfigureAwait(false);
    }

    [HttpGet("current")]
    [Authorize]
    public async Task<UserClaims> GetCurrent()
    {
        var user = await userService.FindUserByNameAsync(User.Identity.Name).ConfigureAwait(false);
        var claims = await claimsService.GetAppClaimsModelForUserAsync(user.Id)
            .ToArrayAsync()
            .ConfigureAwait(false);

        return new UserClaims
        {
            User = new User
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email,
                Name = user.UserName
            },
            Claims = claims
        };
    }
}
