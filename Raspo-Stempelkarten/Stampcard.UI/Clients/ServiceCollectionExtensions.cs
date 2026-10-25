using System.Net;
using System.Net.Security;
using Duende.AccessTokenManagement.OpenIdConnect;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Interceptors;

namespace Stampcard.UI.Clients;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackendClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<TeamHttpClient>(sp => !TryGetRestClient(configuration, sp, out var restClient) 
            ? throw new InvalidOperationException("Could not create backend client for teams.") 
            : new TeamHttpClient(restClient!));
        services.AddTransient<PlayerHttpClient>(sp => !TryGetRestClient(configuration, sp, out var restClient) 
            ? throw new InvalidOperationException("Could not create backend client for teams.") 
            : new PlayerHttpClient(restClient!));
        services.AddTransient<StampCardHttpClient>(sp => !TryGetRestClient(configuration, sp, out var restClient) 
            ? throw new InvalidOperationException("Could not create backend client for teams.") 
            : new StampCardHttpClient(restClient!));
        // external users are anonymous, so this client must not request an access token
        services.AddTransient<SharedStampCardHttpClient>(sp => new SharedStampCardHttpClient(new RestClient(
            new RestClientOptions(configuration.GetConnectionString("Backend")!)
            {
                RemoteCertificateValidationCallback = CertificateValidationOf(sp)
            })));
        return services;
    }

    /// <summary>
    /// Self-signed certificates of a local backend are accepted during development only,
    /// otherwise the certificate is validated as usual.
    /// </summary>
    private static RemoteCertificateValidationCallback? CertificateValidationOf(IServiceProvider sp)
    {
        return sp.GetRequiredService<IHostEnvironment>().IsDevelopment()
            ? (_, _, _, _) => true
            : null;
    }

    private static bool TryGetRestClient(
        IConfiguration configuration,
        IServiceProvider sp,
        out RestClient? restClient)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
        var tokenResult = httpContextAccessor.HttpContext!.GetUserAccessTokenAsync()
            .ConfigureAwait(true)
            .GetAwaiter()
            .GetResult();
        if (!tokenResult.Succeeded)
        {
            logger.LogError("Failed to get access token");
            restClient = null;
            return false;
        }

        var backendUrl = configuration.GetConnectionString("Backend")!;
        logger.LogInformation("Setup RestClient with backend url {Url}", backendUrl);
        var restClientOptions = new RestClientOptions
        {
            BaseUrl = new Uri(backendUrl),
            Interceptors = [new UnauthorizedRedirectInterceptor(httpContextAccessor)],
            Authenticator = new JwtAuthenticator(tokenResult.Token.AccessToken),
            RemoteCertificateValidationCallback = CertificateValidationOf(sp)
        };
        restClient = new RestClient(restClientOptions);
        return true;
    }
}

internal class UnauthorizedRedirectInterceptor(IHttpContextAccessor httpContextAccessor) : Interceptor
{
    public override ValueTask AfterRequest(RestResponse response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException();
        }
        
        return base.AfterRequest(response, cancellationToken);
    }
    
    
}