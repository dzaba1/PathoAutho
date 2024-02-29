namespace Dzaba.PathoAutho.Client;

/// <summary>
/// Root PathoAutho client settings.
/// </summary>
public sealed class PathoClientSettings
{
    /// <summary>
    /// Base URL to the PathoAutho HTTP service.
    /// </summary>
    public Uri BaseUrl { get; set; }

    /// <summary>
    /// Current application ID.
    /// </summary>
    public Guid ApplicationId { get; set; }
}
