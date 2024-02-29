using Dzaba.PathoAutho.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logs\PathoAutho.log");
var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";

var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(LogEventLevel.Information, outputTemplate: outputTemplate)
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, outputTemplate: outputTemplate)
    .CreateLogger();
builder.Services.AddLogging(l => l.AddSerilog(logger, true));

// Add services to the container.
builder.Services.RegisterDzabaPathoAuthoLib(o => o.UseSqlServer(GetConnectionString(builder.Configuration)));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
    {
        Description = "Basic auth added to authorization header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "basic",
        Type = SecuritySchemeType.Http
    });

    s.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IDbInit>().InitAsync().ConfigureAwait(false);
}

app.Run();

string GetConnectionString(IConfiguration configuration)
{
    var envConnStr = Environment.GetEnvironmentVariable("PATHOAUTHO_CONNECTION_STRING");
    if (string.IsNullOrWhiteSpace(envConnStr))
    {
        return configuration.GetConnectionString("Default");
    }

    return envConnStr;
}