using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace TSG_Commex_FE.Services;

/// <summary>
/// DelegatingHandler that automatically adds the JWT access token to outgoing HTTP requests
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuthTokenHandler> _logger;

    public AuthTokenHandler(IServiceProvider serviceProvider, ILogger<AuthTokenHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // Create a scope to get scoped services
            using var scope = _serviceProvider.CreateScope();
            var httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            
            if (httpContextAccessor.HttpContext != null)
            {
                // Get the access token from the authenticated user
                var accessToken = await httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                
                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Add the Bearer token to the Authorization header
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    _logger.LogDebug("Added Bearer token to request to {Url}", request.RequestUri);
                }
                else
                {
                    _logger.LogWarning("No access token available for request to {Url}", request.RequestUri);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding auth token to request to {Url}", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}