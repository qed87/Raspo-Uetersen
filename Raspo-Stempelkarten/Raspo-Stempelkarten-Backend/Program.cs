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
using Raspo_Stempelkarten_Backend.Commands.DeletePlayer;
using Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Commands.EraseStamp;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Commands.StampStampCard;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Mappings;
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
builder.Services.AddScoped<IRequestHandler<StampStampCard, Task<Result<StampStampCardResponse>>>, StampStampCardRequestHandler>();
builder.Services.AddScoped<IRequestHandler<EraseStamp, Task<Result<EraseStampResponse>>>, EraseStampRequestHandler>();
builder.Services.AddScoped<IRequestHandler<CreateStampCard, Task<Result<CreateStampCardResponse>>>, CreateStampCardRequestHandler>();
builder.Services.AddScoped<IRequestHandler<DeleteStampCard, Task<Result<DeleteStampCardResponse>>>, DeleteStampCardRequestHandler>();
builder.Services.AddScoped<IRequestHandler<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>, GetStampCardDetailsQueryHandler>();
builder.Services.AddScoped<IRequestHandler<GetStampCardQuery, Task<StampCardReadDto?>>, GetStampCardQueryHandler>();
builder.Services.AddScoped<IStreamRequestHandler<ListStampCardsQuery, StampCardReadDto>, ListStampCardQueryHandler>();

// Common
builder.Services.AddScoped<IStampModelLoader, StampModelLoader>();
builder.Services.AddTransient<IStampModelStorage, StampModelStorage>();
builder.Services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);
//builder.Services.AddDispatchR(typeof(Program).Assembly, withNotifications: true);
builder.Services.AddDispatchR(options => { });
TypeAdapterConfig.GlobalSettings.Apply(new DefaultRegister());
    
var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();

// var kurrentDbProjection = app.Services.GetRequiredService<KurrentDBProjectionManagementClient>();
// await kurrentDbProjection.EnableAsync("$by_category");
//
// var projections = kurrentDbProjection.ListContinuousAsync();
// if (!await projections.AnyAsync(projection => projection.Name == "StampCard-Seasons-and-Teams"))
// {
//     await kurrentDbProjection.CreateContinuousAsync("StampCard-Seasons-and-Teams", """
//         fromCategory('StampCard')
//         .when({
//             $init: function () {
//                 return {
//                     seasons: {}
//                 }
//             },
//             StampCardCreated: function (state, event) {
//                 if (!event) return state;
//                 let seasons = Object.keys(state.seasons);
//                 let seasonSet = new Set(seasons);
//                 seasonSet.add(event.data.Season);
//                 let newState = { seasons: { } } ;
//                 for (const season of seasonSet) {
//                     let teamResult = state.seasons[season];
//                     if (!teamResult) {
//                         teamResult = { teams: [] };
//                     }
//                     let teamSet = new Set(teamResult.teams);
//                     teamSet.add(event.data.Team);
//                     newState.seasons[season] = {};
//                     newState.seasons[season].teams = [...teamSet];
//                 }
//                 return newState;
//             }
//         })
//         .outputState();
//         """);
// }

await app.RunAsync();