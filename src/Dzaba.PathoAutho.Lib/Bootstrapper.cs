using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.PathoAutho.Lib
{
    public static class Bootstrapper
    {
        public static void RegisterDzabaPathoAuthoLib(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));
            ArgumentNullException.ThrowIfNull(optionsAction, nameof(optionsAction));

            services.AddDbContext<AppDbContext>(optionsAction);
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddTransient<IRegisterService, RegisterService>();
        }
    }
}
