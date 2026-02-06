using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using RestSharp;
using RestSharp.Authenticators;
using Stampcard.UI.Clients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<TeamHttpClient>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var tokenResult = httpContextAccessor.HttpContext!.GetUserAccessTokenAsync()
        .ConfigureAwait(true)
        .GetAwaiter()
        .GetResult();
    if (!tokenResult.Succeeded) throw new InvalidOperationException("Failed to get access token");
    var restClientOptions = new RestClientOptions
    {
        BaseUrl = new Uri(builder.Configuration.GetConnectionString("Backend")!),
        Authenticator = new JwtAuthenticator(tokenResult.Token.IdentityToken.GetValueOrDefault()),
        RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
    };
    var restClient = new RestClient(restClientOptions);
    return new TeamHttpClient(restClient);
});
builder.Services.AddTransient<PlayerHttpClient>(sp =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var tokenResult = httpContextAccessor.HttpContext!.GetUserAccessTokenAsync()
        .ConfigureAwait(true)
        .GetAwaiter()
        .GetResult();
    if (!tokenResult.Succeeded) throw new InvalidOperationException("Failed to get access token");
    var restClientOptions = new RestClientOptions
    {
        BaseUrl = new Uri(builder.Configuration.GetConnectionString("Backend")!),
        Authenticator = new JwtAuthenticator(tokenResult.Token.IdentityToken.GetValueOrDefault()),
        RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true
    };
    var restClient = new RestClient(restClientOptions);
    return new PlayerHttpClient(restClient);
});
builder.Services.AddRazorPages();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        var oidcConfig = builder.Configuration.GetSection("OpenIDConnectSettings");
        options.Authority = oidcConfig["Authority"];
        options.ClientId = oidcConfig["ClientId"];
        options.ClientSecret = oidcConfig["ClientSecret"];

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.MapInboundClaims = true;
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.Scope.Add("offline_access");
    });
builder.Services.AddOpenIdConnectAccessTokenManagement();

// Force authenticated users for whole application
var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(requireAuthPolicy);

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

await app.RunAsync();