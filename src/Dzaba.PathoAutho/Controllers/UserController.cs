﻿using Dzaba.PathoAutho.ActionFilters;
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
    private readonly IModelHelper modelHelper;

    public UserController(IUserService userService,
        IModelHelper modelHelper)
    {
        this.userService = userService;
        this.modelHelper = modelHelper;
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
    public async Task<UserWithApplicationsPermission> GetCurrent()
    {
        return await modelHelper.GetForCurrentUserAsync(User)
            .ConfigureAwait(false);
    }
}
