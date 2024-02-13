using Dzaba.PathoAutho.Lib.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.PathoAutho.Lib;

public static class Bootstrapper
{
    public static void RegisterDzabaPathoAuthoLib(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(optionsAction, nameof(optionsAction));

        services.AddDbContext<AppDbContext>(optionsAction);
        services.AddDefaultIdentity<PathoIdentityUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddTransient<IUserService, UserService>();
    }
}
