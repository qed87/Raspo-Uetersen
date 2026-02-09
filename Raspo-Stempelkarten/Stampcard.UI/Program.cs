using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using RestSharp;
using RestSharp.Authenticators;
using Stampcard.UI.Clients;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddCommandLine(args);
builder.Services.AddBackendClients(builder.Configuration);
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
        
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                // Logge den redirect_uri
                var redirectUri = context.ProtocolMessage.RedirectUri;
                context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("OIDC")
                    .LogInformation("Redirect URI sent to IDP: {RedirectUri}", redirectUri);

                return Task.CompletedTask;
            },

            // Optional: Logge auch bei Fehlern
            OnRemoteFailure = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 500;
                context.Response.WriteAsync("OIDC Failure: " + context.Failure?.Message);
                return Task.CompletedTask;
            }
        };
        
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        
        options.MapInboundClaims = true;
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
    });
builder.Services.AddOpenIdConnectAccessTokenManagement();

// Force authenticated users for whole application
var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(requireAuthPolicy);

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost, 
    RequireHeaderSymmetry = false,
    KnownNetworks = { new IPNetwork(System.Net.IPAddress.Parse("172.19.0.0"), 16) },
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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