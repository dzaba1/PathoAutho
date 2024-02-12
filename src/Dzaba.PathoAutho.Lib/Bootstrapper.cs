﻿using Dzaba.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.PathoAutho.Lib
{
    public static class Bootstrapper
    {
        public static void RegisterDzabaPathoAuthoLib(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        {
            Require.NotNull(services, nameof(services));
            Require.NotNull(optionsAction, nameof(optionsAction));

            services.AddDbContext<AppDbContext>(optionsAction);
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>();

            services.AddTransient<IRegisterService, RegisterService>();
        }
    }
}