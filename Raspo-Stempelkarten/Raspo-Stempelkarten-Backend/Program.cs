using Castle.DynamicProxy;
using DispatchR.Abstractions.Notification;
using DispatchR.Abstractions.Send;
using DispatchR.Abstractions.Stream;
using DispatchR.Extensions;
using FluentResults;
using FluentValidation;
using KurrentDB.Client;
using Mapster;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.CreateStampCard;
using Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;
using Raspo_Stempelkarten_Backend.Commands.DeletePlayer;
using Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Commands.EraseStamp;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Commands.StampStampCard;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Mappings;
using Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetPlayer;
using Raspo_Stempelkarten_Backend.Queries.GetStampCard;
using Raspo_Stempelkarten_Backend.Queries.GetStampCardDetails;
using Raspo_Stempelkarten_Backend.Queries.ListPlayers;
using Raspo_Stempelkarten_Backend.Queries.ListStampCards;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddMapster();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<EventDataChangeTracker>();

// Change Tracker 
builder.Services.AddScoped<IEventDataChangeTracker, EventDataChangeTracker>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddScoped<INotificationHandler<PlayerAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddScoped<INotificationHandler<PlayerDeleted>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddScoped<INotificationHandler<TeamAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddScoped<INotificationHandler<StampCardAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddScoped<INotificationHandler<StampCardRemoved>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddScoped<INotificationHandler<StampAdded>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddScoped<INotificationHandler<StampErased>>(provider => provider.GetRequiredService<EventDataChangeTracker>());
builder.Services.AddSingleton<IProxyGenerator, ProxyGenerator>();

// Commands Teams
builder.Services.AddScoped<IRequestHandler<AddTeamRequest, Task<Result<AddTeamResponse>>>, AddTeamRequestHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteTeamRequest, Task<Result>>, DeleteTeamRequestHandler>();

// Commands/Queries Players
builder.Services.AddScoped<IRequestHandler<AddPlayersRequest, Task<Result<AddPlayersResponse>>>, AddPlayersRequestHandler>();
builder.Services.AddScoped<IRequestHandler<DeletePlayerRequest, Task<Result<DeletePlayerResponse>>>, DeletePlayerRequestHandler>();
builder.Services.AddScoped<IStreamRequestHandler<ListPlayersQuery, PlayerReadDto>, ListPlayerStreamQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetPlayersQuery, Task<PlayerReadDto?>>, GetPlayerQueryHandler>();

// Commands/Queries Stamp Card
builder.Services.AddScoped<IRequestHandler<CreateTeamStampCardsForAccountingYears, Task<Result<CreateTeamStampCardsForAccountingYearsResponse>>>, 
    CreateTeamStampCardsForAccountingYearsRequestHandler>();
builder.Services.AddScoped<IRequestHandler<StampStampCard, Task<Result<StampStampCardResponse>>>, StampStampCardRequestHandler>();
builder.Services.AddScoped<IRequestHandler<EraseStamp, Task<Result<EraseStampResponse>>>, EraseStampRequestHandler>();
builder.Services.AddScoped<IRequestHandler<CreateStampCard, Task<Result<CreateStampCardResponse>>>, CreateStampCardRequestHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteStampCard, Task<Result<DeleteStampCardResponse>>>, DeleteStampCardRequestHandler>();
builder.Services.AddScoped<IRequestHandler<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>, GetStampCardDetailsQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetStampCardQuery, Task<StampCardReadDto?>>, GetStampCardQueryHandler>();
builder.Services.AddScoped<IStreamRequestHandler<ListStampCardsQuery, StampCardReadDto>, ListStampCardQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetCompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, GetCompletedStampCardsQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetIncompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>, GetIncompletedStampCardsQueryHandler>();

// Common
builder.Services.AddScoped<IStampModelLoader, StampModelLoader>();
builder.Services.AddTransient<IStampModelStorage, StampModelStorage>();
builder.Services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);

builder.Services.AddDispatchR(options => { });
TypeAdapterConfig.GlobalSettings.Apply(new DefaultRegister());
    
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();

var kurrentDbProjection = app.Services.GetRequiredService<KurrentDBProjectionManagementClient>();
await kurrentDbProjection.EnableAsync("$by_event_type");

var projections = kurrentDbProjection.ListContinuousAsync();
if (!await projections.AnyAsync(projection => projection.Name == "Team-Streams"))
{
    await kurrentDbProjection.CreateContinuousAsync("Teams-Stream", """
        fromStream('$et-TeamAdded')
        .when({
            $init: function () {
                return {
                    streams: []
                }
            },
            $any: function (state, event) {
                if (!event.data) return state;
                if (event.eventType !== "TeamAdded") return state;
                log("Event: " + JSON.stringify(event.data));
                state.streams.push(event.data.Club + "-" + event.data.BirthCohort);
                return state;
            }
        })
        .outputState();
        """);
}

await app.RunAsync();