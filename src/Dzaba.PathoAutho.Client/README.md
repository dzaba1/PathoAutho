# Dzaba PathoAutho client library and authentication handler

## Usage

Install nuget https://www.nuget.org/packages/Dzaba.PathoAutho.Client/

In Startup or Program.cs:
```
var builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddPathoAuthoClient(GetPathoAuthoSettings);

builder.Services.AddAuthentication(o =>
{
    o.AddPathoAuthoBasicAuthScheme(true);
});

builder.Services.AddAuthorization();
```