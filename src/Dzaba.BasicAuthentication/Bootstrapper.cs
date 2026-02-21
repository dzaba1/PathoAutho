using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.BasicAuthentication;

/// <summary>
/// Extensions for installing services into Ioc container
/// </summary>
public static class Bootstrapper
{
    /// <summary>
    /// Adds the basic authentication handler
    /// </summary>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    public static IServiceCollection AddBasicAuthentication<THandler>(this IServiceCollection services)
        where THandler : class, IBasicAuthenticationHandlerService
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IBasicAuthenticationHandlerService, THandler>();

        return services;
    }

    /// <summary>
    /// Adds the basic authentication scheme.
    /// </summary>
    /// <param name="options">Authentication options.</param>
    /// <param name="isDefault">If checked then basic authentication shceme will be a default one.</param>
    /// <returns>Authentication options.</returns>
    public static AuthenticationOptions AddBasicAuthenticationScheme(this AuthenticationOptions options,
        bool isDefault = false)
    {
        ArgumentNullException.ThrowIfNull(options);

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
