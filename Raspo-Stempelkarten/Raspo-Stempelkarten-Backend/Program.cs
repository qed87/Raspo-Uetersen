using DispatchR.Abstractions.Notification;
using DispatchR.Abstractions.Send;
using DispatchR.Extensions;
using FluentResults;
using FluentValidation;
using KurrentDB.Client;
using Mapster;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Mappings;
using Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;
using Raspo_Stempelkarten_Backend.Queries.StampCardStamp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddMapster();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IRequestHandler<StampCardGetByIdQuery, Task<Result<StampCardReadDto>>>, StampCardGetByIdQueryHandler>();
builder.Services.AddTransient<IRequestHandler<StampCardDetailedGetByIdQuery, Task<Result<StampCardReadDetailsDto>>>, StampCardGetByIdQueryHandler>();
builder.Services.AddTransient<IRequestHandler<StampCardStampGetByIdQuery, Task<Result<StampReadDto>>>, StampCardStampQueryHandler>();
builder.Services.AddTransient<IRequestHandler<StampCardStampListQuery, Task<Result<IEnumerable<StampReadDto>>>>, StampCardStampQueryHandler>();
builder.Services.AddScoped<StampCardChangeHandler>();
builder.Services.AddScoped<INotificationHandler<StampCardCreated>, StampCardChangeHandler>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<INotificationHandler<StampCardDeleted>, StampCardChangeHandler>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<INotificationHandler<StampCardPropertyChanged>, StampCardChangeHandler>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<INotificationHandler<StampCardOwnerAdded>, StampCardChangeHandler>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<INotificationHandler<StampCardOwnerRemoved>, StampCardChangeHandler>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<INotificationHandler<StampCardStamped>, StampCardChangeHandler>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<INotificationHandler<StampCardStampErased>, StampCardChangeHandler>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<IStampCardChangeTracker>(cfg => cfg.GetRequiredService<StampCardChangeHandler>());
builder.Services.AddScoped<IStampCardModelLoader, StampCardModelLoader>();
builder.Services.AddScoped<IStreamNameProvider, StreamNameProvider>();
builder.Services.AddScoped<IStampCardModelStorage, StampCardModelStorage>();
builder.Services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);
builder.Services.AddDispatchR(typeof(Program).Assembly, withNotifications: true);

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

var projections = kurrentDbProjection.ListContinuousAsync();
if (!await projections.AnyAsync(projection => projection.Name == "StampCard-Teams-and-Seasons"))
{
    await kurrentDbProjection.CreateContinuousAsync("StampCard-Teams-and-Seasons", """
        fromCategory('StampCard')
            .when({
                $init: function () {
                    return {
                        teams: new Set(),
                        season: new Set()
                    }
                },
                StampCardCreated: function (state, event) {
                    state.teams.add(event.Team);
                    state.seasons.add(event.Season);
                }
            })
            .outputState()
        """);
}

await app.RunAsync();