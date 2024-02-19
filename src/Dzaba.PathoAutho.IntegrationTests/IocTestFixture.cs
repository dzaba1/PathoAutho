using Dzaba.PathoAutho.Contracts;
using Dzaba.PathoAutho.Lib;
using Dzaba.PathoAutho.Lib.Model;
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
    public async Task SetupContainerAsync()
    {
        var services = new ServiceCollection();
        services.RegisterDzabaPathoAuthoLib(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        var logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
        services.AddLogging(l => l.AddSerilog(logger, true));

        container = services.BuildServiceProvider();

        await Container.GetRequiredService<IDbInit>().InitAsync().ConfigureAwait(false);
    }

    [TearDown]
    public void DisposeContainer()
    {
        container?.Dispose();
    }

    protected async Task<PathoIdentityUser> AddUserAsync(string email = "test@test.com", string password = "Password1!")
    {
        var model = new RegisterUser
        {
            Email = email,
            Password = password
        };

        var userMgr = Container.GetRequiredService<IUserService>();
        await userMgr.RegisterAsync(model).ConfigureAwait(false);

        return await userMgr.FindUserByNameAsync(model.Email).ConfigureAwait(false);
    }

    protected IApplicationService GetApplicationService()
    {
        return Container.GetRequiredService<IApplicationService>();
    }

    protected IRoleService GetRolesService()
    {
        return Container.GetRequiredService<IRoleService>();
    }

    protected IClaimService GetClaimService()
    {
        return Container.GetRequiredService<IClaimService>();
    }
}
