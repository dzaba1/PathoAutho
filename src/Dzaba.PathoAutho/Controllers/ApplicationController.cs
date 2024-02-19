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

    [HttpGet("{appId}/admin")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task<User[]> GetAdminsAsync(Guid appId)
    {
        return await roleService.GetAdmins(appId)
            .Select(u => u.ToModel())
            .ToArrayAsync()
            .ConfigureAwait(false);
    }

    [HttpGet("{appId}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task<ApplicationData> GetAppData(Guid appId)
    {
        var app = await appService.GetApplicationAsync(appId)
            .ConfigureAwait(false);

        if (app == null)
        {
            throw new HttpResponseException(HttpStatusCode.BadRequest, $"App with ID {appId} doesn't exist.");
        }

        var admins = await roleService.GetAdmins(appId)
            .Select(u => u.ToModel())
            .ToArrayAsync()
            .ConfigureAwait(false);

        var rolesQuery = from r in dbContext.PathoRoles
                         join ur in dbContext.PathoUserRoles on r.Id equals ur.RoleId
                         join u in dbContext.Users on ur.UserId equals u.Id
                         where r.ApplicationId == appId
                         select new { r, u };
        var roles = rolesQuery.GroupBy(r => r.r, r => r.u).ToAsyncEnumerable();

        var permissionsQuery = from p in dbContext.Permissions
                         join up in dbContext.UserPermissions on p.Id equals up.PermissionId
                         join u in dbContext.Users on up.UserId equals u.Id
                         where p.ApplicationId == appId
                         select new { p, u };
        var permissions = permissionsQuery.GroupBy(p => p.p, p => p.u).ToAsyncEnumerable();

        return new ApplicationData
        {
            Application = app.ToModel(),
            Admins = admins,
            Roles = await roles.Select(r => new ClaimAssignment
            {
                Claim = r.Key.ToModel(),
                Users = r.Select(u => u.ToModel()).ToArray()
            }).ToArrayAsync().ConfigureAwait(false),
            Permissions = await permissions.Select(r => new ClaimAssignment
            {
                Claim = r.Key.ToModel(),
                Users = r.Select(u => u.ToModel()).ToArray()
            }).ToArrayAsync().ConfigureAwait(false)
        };
    }
}
