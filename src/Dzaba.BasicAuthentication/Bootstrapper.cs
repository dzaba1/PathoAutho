using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.BasicAuthentication;

public static class Bootstrapper
{
    public static void AddBasicAuthentication<THandler>(this IServiceCollection services)
        where THandler : class, IBasicAuthenticationHandlerService
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddTransient<IBasicAuthenticationHandlerService, THandler>();
    }

    public static AuthenticationOptions AddBasicAuthenticationScheme(this AuthenticationOptions options,
        bool isDefault = false)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        options.AddScheme<BasicAuthenticationHandler>(Constants.SchemeName, "Basic authentication");

        if (isDefault)
        {
            options.DefaultAuthenticateScheme = Constants.SchemeName;
            options.DefaultChallengeScheme = Constants.SchemeName;
            options.DefaultSignInScheme = Constants.SchemeName;
        }
        
        return options;
    }
}
