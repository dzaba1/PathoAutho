using Dzaba.PathoAutho.Client;
using Serilog.Events;
using Serilog;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logs\PathoAuthoTestApp.log");
var outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}";

var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(LogEventLevel.Information, outputTemplate: outputTemplate)
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, outputTemplate: outputTemplate)
    .CreateLogger();
builder.Services.AddLogging(l => l.AddSerilog(logger, true));

builder.Services.AddPathoAuthoClient(GetPathoAuthoSettings);

builder.Services.AddAuthentication(o =>
{
    o.AddPathoAuthoBasicAuthScheme(true);
});

builder.Services.AddAuthorization();

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

app.Run();

PathoClientSettings GetPathoAuthoSettings(IServiceProvider provider)
{
    var config = provider.GetRequiredService<IConfiguration>();
    var section = config.GetSection("PathoAutho");
    return section.Get<PathoClientSettings>();
}