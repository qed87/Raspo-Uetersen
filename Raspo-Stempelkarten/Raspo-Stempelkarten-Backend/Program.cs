using FluentValidation;
using KurrentDB.Client;
using LiteBus.Commands.Extensions.MicrosoftDependencyInjection;
using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using Mapster;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using StempelkartenModelLoader = Raspo_Stempelkarten_Backend.Commands.Shared.StempelkartenModelLoader;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddMapster();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IStempelkartenModelLoader, StempelkartenModelLoader>();
builder.Services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);
builder.Services.AddLiteBus(registry =>
{
    registry.AddCommandModule(moduleBuilder => moduleBuilder.RegisterFromAssembly(typeof(Program).Assembly));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
