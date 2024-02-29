using Dzaba.BasicAuthentication;
using Dzaba.PathoAutho.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace Dzaba.PathoAutho.Client
{
    internal sealed class PathoBasicAuthHandler : IBasicAuthenticationHandlerService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private readonly PathoClientSettings settings;
        private readonly ILogger<PathoBasicAuthHandler> logger;

        public PathoBasicAuthHandler(PathoClientSettings settings,
            ILogger<PathoBasicAuthHandler> logger)
        {
            ArgumentNullException.ThrowIfNull(settings, nameof(settings));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            this.settings = settings;
            this.logger = logger;
        }

        public Task AddClaimsAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext,
            ICollection<System.Security.Claims.Claim> claims, object context)
        {
            var permissions = (ApplicationPermissionsWithUser)context;

            claims.Add(new System.Security.Claims.Claim("UserId", permissions.User.Id.ToString(), ClaimValueTypes.String));
            claims.Add(new System.Security.Claims.Claim(ClaimTypes.Email, permissions.User.Email, ClaimValueTypes.Email));

            foreach (var role in permissions.Roles)
            {
                claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, role.Name));
            }

            foreach (var claimModel in permissions.Claims)
            {
                claims.Add(new System.Security.Claims.Claim(claimModel.Type, claimModel.Value));
            }

            return Task.CompletedTask;
        }

        public async Task<CheckPasswordResult> CheckPasswordAsync(BasicAuthenticationCredentials credentials, HttpContext httpContext)
        {
            var url = new Uri(settings.BaseUrl, $"user/current/application/{settings.ApplicationId}");

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization = credentials.ToHeaderValue();

                logger.LogDebug("GET {Url}", url);
                using var resp = await httpClient.SendAsync(req).ConfigureAwait(false);
                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new CheckPasswordResult("Invalid username or password.");
                }
                resp.EnsureSuccessStatusCode();

                var contentStr = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                var permissions = JsonSerializer.Deserialize<ApplicationPermissionsWithUser>(contentStr, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return CheckPasswordResult.Success(permissions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calling PathoAutho {Url}", url);
                return new CheckPasswordResult(ex.Message);
            }
        }

        public async Task HandleUnauthorizedAsync(HttpContext httpContext, string failReason)
        {
            ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));
            ArgumentException.ThrowIfNullOrWhiteSpace(failReason, nameof(failReason));

            await httpContext.Response.WriteAsync(failReason).ConfigureAwait(false);
        }
    }
}
