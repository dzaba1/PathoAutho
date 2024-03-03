# Dzaba Basic Authentication handler

## Usage

Install nuget https://www.nuget.org/packages/Dzaba.BasicAuthentication/

In Startup or Program.cs:
```
var builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddBasicAuthentication<MyHandler>();

builder.Services.AddAuthentication(o =>
{
    o.AddBasicAuthenticationScheme(true);
});

builder.Services.AddAuthorization();
```

Where `MyHandler` is your custom implementation. Example:
```
internal sealed class MyHandler : IBasicAuthenticationHandlerService
{
    private readonly IMyUserService userService;

    public MyHandler(IMyUserService userService)
    {
        this.userService = userService;
    }

    public async Task AddClaimsAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext, ICollection<Claim> claims, object context)
    {
        var userModel = (UserModel)context;

        await foreach (var role in userService.GetRolesAsync(userModel))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
    }

    public async Task<CheckPasswordResult> CheckPasswordAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext)
    {
        var userModel = await userService.GetUserModelAsync(credentials.UserName);
        if (userModel == null)
        {
            return new CheckPasswordResult("Invalid user name.");
        }

        if (await userService.IsPasswordOkAsync(userModel, credentials.Password))
        {
            return CheckPasswordResult.Success(userModel);
        }

        return new CheckPasswordResult("Invalid user name or password.");
    }

    public async Task HandleUnauthorizedAsync(HttpContext httpContext, string failReason)
    {
        await httpContext.Response.WriteAsync(failReason).ConfigureAwait(false);
    }
}
```