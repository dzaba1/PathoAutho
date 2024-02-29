using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dzaba.PathoAutho.TestApp.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public UserAndClaims Get()
    {
        return new UserAndClaims
        {
            Id = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value,
            Name = User.Identity.Name,
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
            Claims = User.Claims.Select(c => new ClaimModel
            {
                Type = c.Type,
                Value = c.Value,
                ValueType = c.ValueType
            }).ToArray()
        };
    }

    [HttpGet("role")]
    [Authorize(Roles = "TestRole")]
    public UserAndClaims TestRole()
    {
        return new UserAndClaims
        {
            Id = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value,
            Name = User.Identity.Name,
            Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
            Claims = User.Claims.Select(c => new ClaimModel
            {
                Type = c.Type,
                Value = c.Value,
                ValueType = c.ValueType
            }).ToArray()
        };
    }
}
