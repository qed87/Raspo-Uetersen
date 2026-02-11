using System.Net;
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
        return services;
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
            RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
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