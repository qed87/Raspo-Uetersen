using FluentValidation;
using KurrentDB.Client;
using LiteBus.Commands.Extensions.MicrosoftDependencyInjection;
using LiteBus.Events.Extensions.MicrosoftDependencyInjection;
using LiteBus.Messaging.Extensions.MicrosoftDependencyInjection;
using LiteBus.Queries.Extensions.MicrosoftDependencyInjection;
using Mapster;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Mappings;
using StempelkartenModelLoader = Raspo_Stempelkarten_Backend.Commands.Shared.StempelkartenModelLoader;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddMapster();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<StampCardChangeTracker>();
builder.Services.AddTransient<IStempelkartenReplayer, StempelkartenReplayer>();
builder.Services.AddSingleton<IStempelkartenModelLoader, StempelkartenModelLoader>();
builder.Services.AddSingleton<IStreamNameProvider, StreamNameProvider>();
builder.Services.AddSingleton<IStempelkartenModelStorage, StempelkartenModelStorage>();
builder.Services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);
builder.Services.AddLiteBus(registry =>
{
    registry.AddCommandModule(moduleBuilder => moduleBuilder.RegisterFromAssembly(typeof(Program).Assembly));
    registry.AddQueryModule(moduleBuilder => moduleBuilder.RegisterFromAssembly(typeof(Program).Assembly));
    registry.AddEventModule(moduleBuilder => moduleBuilder.RegisterFromAssembly(typeof(Program).Assembly));
});

TypeAdapterConfig.GlobalSettings.Apply(new DefaultRegister());
    
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

var kurrentDbProjection = app.Services.GetRequiredService<KurrentDBProjectionManagementClient>();
await kurrentDbProjection.EnableAsync("$by_category");

await kurrentDbProjection.CreateContinuousAsync("Stempelkarten-Teams-and-Saisons", """
fromCategory('Stempelkarten')
    .when({
        $init: function () {
            return {
                teams: new Set();
                season: new Set();
            }
        },
        StampCardCreated: function (state, event) {
            state.teams.add(event.Team);
            state.seasons.add(event.Season);
        }
    })
    .outputState()
""");

await app.RunAsync();