using FluentValidation;
using KurrentDB.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using StampCard.Backend;
using StampCard.Backend.Authorization;
using StampCard.Backend.Dtos;
using StampCard.Backend.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddCommandLine(args);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSerilog();

// Application specific services
builder.Services.AddApplicationServices(builder.Configuration);
var jwtConfiguration = builder.Configuration.GetSection("Jwt");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = jwtConfiguration.GetValue<string>("Authority");
    options.Audience = jwtConfiguration.GetValue<string>("Audience");
    options.RequireHttpsMetadata = jwtConfiguration.GetValue<bool>("RequireHttpsMetadata"); // Only for develop
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = jwtConfiguration.GetValue<bool>("ValidateIssuer"),
        ValidIssuer = jwtConfiguration.GetValue<string>("Authority"),
        ValidateAudience = jwtConfiguration.GetValue<bool>("ValidateAudience"),
        ValidAudience = jwtConfiguration.GetValue<string>("Audience"),
        NameClaimType = jwtConfiguration.GetValue<string>("NameClaimType")
    };
});

builder.Services.AddSingleton<IAuthorizationHandler, TeamCoachRequirementHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("IsManager", policy => policy.RequireRole("manager"))
    .AddPolicy("TeamCoachOnly", policy => policy.Requirements.Add(new TeamCoachRequirement()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    const string schemeName = "Bearer";

    c.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme
    {
        Type        = SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT",
        In          = ParameterLocation.Header,
        Name        = "Authorization",
        Description = "JWT Authorization header.<br/>Enter **only** the token (without 'Bearer ' prefix)."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(schemeName, document)] = []
    });
});

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ModelConcurrencyException modelConcurrencyException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(ResponseWrapperDto.Fail(modelConcurrencyException.Message));
    }
    catch (ModelLoadException modelLoadException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ResponseWrapperDto.Fail("Unbekannter Fehler beim Laden der Geschäftsdaten."));
    }
    catch (ModelException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ResponseWrapperDto.Fail("Unerwarteter Fehler in der Geschäftslogik aufgetreten."));
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ResponseWrapperDto.Fail("Schwerwiegender Fehler: Bitte informieren Sie Ihren Systemadministrator."));
    }
});
app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.OAuthScopes("openid", "profile", "email");
    });
}

app.MapSwagger();

var kurrentDbProjection = app.Services.GetRequiredService<KurrentDBProjectionManagementClient>();
const int maxRetries = 10;
for (var i = 0; i < maxRetries; i++)
{
    try
    {
        await Task.Delay((int) (Math.Pow(2, i) - 1) * 250);
        var response = await kurrentDbProjection.GetStatusAsync("$by_category");
        if (response is null) continue;
        if (response.Status.Contains("Running"))
        {
            var projections = kurrentDbProjection.ListContinuousAsync();
            if (await projections.AllAsync(projection => projection.Name != "all-club-teams"))
            {
                await kurrentDbProjection.CreateContinuousAsync("all-club-teams", """
                    fromCategory('team')
                        .when({
                            $init: function () {
                                return {
                                    teams: []
                                }
                            },
                            TeamAdded: function (state, event) {
                                if (event.eventType !== "TeamAdded") return state;
                                if (!event.data) return state;
                                log("Event: " + JSON.stringify(event.data));
                                state.teams.push({ "club": event.data.Club, "name": event.data.Name, "id": event.streamId, "coaches": [] });
                                return state;
                            },
                            TeamUpdated: function (state, event) {
                                if (event.eventType !== "TeamUpdated") return state;
                                if (!event.data) return state;
                                log("Event: " + JSON.stringify(event.data));
                                const index = state.teams.findIndex((team)  => event.streamId === team.id);
                                if (index == -1) return state;
                                state.teams[index].name = event.data.Name;
                                return state;
                            },
                            TeamDeleted: function (state, event) {
                                if (event.eventType !== "TeamDeleted") return state;
                                if (!event.data) return state;
                                log("Event: " + JSON.stringify(event.data));
                                const index = state.teams.findIndex((team) => event.streamId === team.id);
                                if (index == -1) return state;
                                state.teams.splice(index, 1);
                                return state;
                            },
                            CoachAdded: function (state, event) {
                                if (event.eventType !== "CoachAdded") return state;
                                if (!event.data) return state;
                                log("Event: " + JSON.stringify(event.data));
                                const index = state.teams.findIndex((team) => event.streamId === team.id);
                                if (index == -1) return state;
                                state.teams[index].coaches.push(event.data.Email);
                                return state;
                            },
                            CoachRemoved: function (state, event) {
                                if (event.eventType !== "CoachRemoved") return state;
                                if (!event.data) return state;
                                log("Event: " + JSON.stringify(event.data));
                                const index = state.teams.findIndex((team) => event.streamId === team.id);
                                if (index == -1) return state;
                                var removeCoachIndex = state.teams[index].coaches.indexOf(event.data.Email);
                                state.teams[index].coaches.splice(removeCoachIndex, 1);
                                return state;
                            }
                        })
                        .outputState();
                    """);
            }
            app.Logger.Log(LogLevel.Information, "Projections successfully initialized.");
            break;
        }
    }
    catch (Exception ex)
    {
        app.Logger.Log(LogLevel.Warning, ex, "Retry initialize projections");
    }
}

await app.RunAsync();