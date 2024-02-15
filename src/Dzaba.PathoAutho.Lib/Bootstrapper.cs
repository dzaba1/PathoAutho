using Dzaba.PathoAutho.Lib.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dzaba.PathoAutho.Lib;

public static class Bootstrapper
{
    public static void RegisterDzabaPathoAuthoLib(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(optionsAction, nameof(optionsAction));

        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseLazyLoadingProxies();
            optionsAction(o);
        });

        services.AddIdentityServicesOnly<PathoIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        //services.AddIdentity<PathoIdentityUser, IdentityRole>()
        //    .AddEntityFrameworkStores<AppDbContext>();

        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ILoginService, LoginService>();

        services.AddAuthentication(o =>
        {
            o.AddScheme<BasicAuthenticationHandler>(BasicAuthenticationHandler.SchemeName, "Basic authentication");

            o.DefaultAuthenticateScheme = BasicAuthenticationHandler.SchemeName;
            o.DefaultChallengeScheme = BasicAuthenticationHandler.SchemeName;
            o.DefaultSignInScheme = BasicAuthenticationHandler.SchemeName;
        });
    }

    public static IdentityBuilder AddIdentityServicesOnly<TUser, TRole>(this IServiceCollection services)
        where TUser : IdentityUser
        where TRole : IdentityRole
    {
        // Hosting doesn't add IHttpContextAccessor by default
        services.AddHttpContextAccessor();
        // Identity services
        services.TryAddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
        services.TryAddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
        services.TryAddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
        services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.TryAddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
        // No interface for the error describer so we can add errors without rev'ing the interface
        services.TryAddScoped<IdentityErrorDescriber>();
        services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<TUser>>();
        //services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<SecurityStampValidatorOptions>, PostConfigureSecurityStampValidatorOptions>());
        services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<TUser>>();
        services.TryAddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
        services.TryAddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
        services.TryAddScoped<UserManager<TUser>>();
        services.TryAddScoped<SignInManager<TUser>>();
        services.TryAddScoped<RoleManager<TRole>>();

        return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
    }
}
