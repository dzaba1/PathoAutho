using Dzaba.PathoAutho.Lib;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"logs\PathoAutho.log");
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(LogEventLevel.Information)
    .WriteTo.File(logFile, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
    .CreateLogger();
builder.Services.AddLogging(l => l.AddSerilog(logger, true));

// Add services to the container.
builder.Services.RegisterDzabaPathoAuthoLib(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<IDbInit>().InitAsync().ConfigureAwait(false);
}

app.Run();
