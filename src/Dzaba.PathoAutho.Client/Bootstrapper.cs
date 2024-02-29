using Dzaba.BasicAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Dzaba.PathoAutho.Client;

/// <summary>
/// Extensions for installing services into Ioc container
/// </summary>
public static class Bootstrapper
{
    /// <summary>
    /// Adds PathoAutho clients into Ioc container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="settingsFactory">Factory method for creating <see cref="PathoClientSettings"/></param>
    public static void AddPathoAuthoClient(this IServiceCollection services,
        Func<IServiceProvider, PathoClientSettings> settingsFactory)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(settingsFactory, nameof(settingsFactory));

        services.AddTransient(settingsFactory);
        services.AddBasicAuthentication<PathoBasicAuthHandler>();
    }

    /// <summary>
    /// Adds the basic authentication scheme.
    /// </summary>
    /// <param name="options">Authentication options.</param>
    /// <param name="isDefault">If checked then basic authentication shceme will be a default one.</param>
    /// <returns>Authentication options.</returns>
    public static AuthenticationOptions AddPathoAuthoBasicAuthScheme(this AuthenticationOptions options,
        bool isDefault = false)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        options.AddBasicAuthenticationScheme(isDefault);
        
        return options;
    }
}
