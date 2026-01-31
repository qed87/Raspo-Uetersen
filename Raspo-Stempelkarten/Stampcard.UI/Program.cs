using RestSharp.Extensions.DependencyInjection;
using Stampcard.UI.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRestClient(options =>
{
    options.BaseUrl = new Uri("https://localhost:7184");
    options.RemoteCertificateValidationCallback = (_, _, _, _) => true;
});
builder.Services.AddTransient<TeamHttpClient>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddRazorPages();

var app = builder.Build();

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
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

await app.RunAsync();