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
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService appService;
    private readonly IRoleService roleService;
    private readonly AppDbContext dbContext;

    public ApplicationController(IApplicationService appService,
        IRoleService roleService,
        AppDbContext dbContext)
    {
        this.appService = appService;
        this.roleService = roleService;
        this.dbContext = dbContext;
    }

    [HttpPost("{appName}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<Guid> NewApplicationAsync(string appName)
    {
        return await appService.NewApplicationAsync(appName)
            .ConfigureAwait(false);
    }

    [HttpDelete("{appId}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task RemoveApplicationAsync(Guid appId)
    {
        await appService.RemoveApplicationAsync(appId)
            .ConfigureAwait(false);
    }

    [HttpPut("{appId}")]
    [ValidateModel]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task ChangeApplicationAsync(Guid appId, [FromBody, Required] ChangeApplication changeApplication)
    {
        await appService.ChangeNameAsync(appId, changeApplication.NewName)
            .ConfigureAwait(false);
    }

    [HttpPost("{appId}/admin/user/{userName}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task SetAdminAsync(Guid appId, string userName)
    {
        await roleService.SetApplicationAdminAsync(userName, appId)
            .ConfigureAwait(false);
    }

    [HttpDelete("{appId}/admin/user/{userName}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task RevokeAdminAsync(Guid appId, string userName)
    {
        await roleService.RevokeApplicationAdminAsync(userName, appId)
            .ConfigureAwait(false);
    }
}
