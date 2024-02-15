using Dzaba.PathoAutho.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Serilog;

namespace Dzaba.PathoAutho.IntegrationTests;

public class IocTestFixture
{
    private ServiceProvider container;

    protected IServiceProvider Container => container;

    [SetUp]
    public void SetupContainer()
    {
        var services = new ServiceCollection();
        services.RegisterDzabaPathoAuthoLib(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        services.AddLogging(l => l.AddSerilog(logger, true));

        container = services.BuildServiceProvider();

        InitDb();
    }

    private void InitDb()
    {
        Container.GetRequiredService<AppDbContext>().Database.EnsureCreated();
    }

    [TearDown]
    public void DisposeContainer()
    {
        container?.Dispose();
    }
}
