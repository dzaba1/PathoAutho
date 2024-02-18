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
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService appService;

    public ApplicationController(IApplicationService appService)
    {
        this.appService = appService;
    }

    [HttpPost("{appName}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<Guid> NewApplicationAsync(string appName)
    {
        return await appService.NewApplicationAsync(appName)
            .ConfigureAwait(false);
    }

    [HttpDelete("{appId}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task RemoveApplicationAsync(Guid appId)
    {
        await appService.RemoveApplicationAsync(appId)
            .ConfigureAwait(false);
    }

    [HttpPut]
    [ValidateModel]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task ChangeApplicationAsync([FromBody, Required] ChangeApplication changeApplication)
    {
        await appService.ChangeNameAsync(changeApplication.Id, changeApplication.NewName)
            .ConfigureAwait(false);
    }
}
