using Castle.DynamicProxy;
using DispatchR.Abstractions.Send;
using DispatchR.Extensions;
using FluentResults;
using FluentValidation;
using KurrentDB.Client;
using Mapster;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Mappings;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddMapster();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IProxyGenerator, ProxyGenerator>();
builder.Services.AddTransient<IRequestHandler<AddPlayersRequest, Task<Result<AddPlayersResponse>>>, AddPlayersRequestHandler>();
builder.Services.AddTransient<IRequestHandler<AddTeamRequest, Task<Result<AddTeamResponse>>>, AddTeamRequestHandler>();
builder.Services.AddTransient<IRequestHandler<DeleteTeamRequest, Task<Result>>, DeleteTeamRequestHandler>();
builder.Services.AddTransient<KurrentDBClient>(_ => new KurrentDBClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddTransient<KurrentDBProjectionManagementClient>(_ => new KurrentDBProjectionManagementClient(
    KurrentDBClientSettings.Create(builder.Configuration.GetConnectionString("KurrentDb")!)));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);
//builder.Services.AddDispatchR(typeof(Program).Assembly, withNotifications: true);
builder.Services.AddDispatchR(options =>
{
});
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