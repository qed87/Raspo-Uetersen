using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Raspo.StampCard.Web.Components;
using Raspo.StampCard.Web.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddBlazorBootstrap();

builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<StampCardService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// app.MapGet("/account/login", async (HttpContext httpContext, string returnUrl = "/") =>
// {
//   var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
//           .WithRedirectUri(returnUrl)
//           .Build();
//
//   await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
// });
//
// app.MapGet("/account/logout", async (HttpContext httpContext) =>
// {
//   var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
//           .WithRedirectUri("/")
//           .Build();
//
//   await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
//   await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
// });

app.Run();