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
public class ClaimController : ControllerBase
{
    private readonly IClaimService claimService;
    private readonly IAuthHelper authHelper;

    public ClaimController(IClaimService claimService,
        IAuthHelper authHelper)
    {
        this.claimService = claimService;
        this.authHelper = authHelper;
    }

    [HttpPost]
    [Authorize]
    [ValidateModel]
    public async Task<int> SetClaim([Required, FromBody] SetClaim newClaim)
    {
        await authHelper.CheckSuperOrAppAdminAsync(User, newClaim.ApplicationId)
            .ConfigureAwait(false);

        return await claimService.SetClaimAsync(newClaim.ApplicationId, newClaim.UserName, newClaim.Type, newClaim.Value)
            .ConfigureAwait(false);
    }

    [HttpDelete("{claimId}")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.AppAdmin)]
    public async Task RemoveClaim(int claimId)
    {
        await claimService.RemoveClaimAsync(claimId).ConfigureAwait(false);
    }
}
