using DispatchR.Extensions;
using FluentValidation;
using Keycloak.AuthServices.Authentication;
using KurrentDB.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Raspo_Stempelkarten_Backend;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

// Application specific services
builder.Services.AddApplicationServices(builder.Configuration);
var jwtConfiguration = builder.Configuration.GetSection("Jwt");
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, ServiceLifetime.Transient);
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.Authority = jwtConfiguration.GetValue<string>("Authority");
    options.RequireHttpsMetadata = jwtConfiguration.GetValue<bool>("RequireHttpsMetadata"); // Only for develop
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = jwtConfiguration.GetValue<bool>("ValidateIssuer"),
        ValidIssuer = jwtConfiguration.GetValue<string>("Authority"),
        ValidateAudience = jwtConfiguration.GetValue<bool>("ValidateAudience"),
        ValidAudience = jwtConfiguration.GetValue<string>("Audience")
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsClubManager", policy => policy.RequireRole("club-manager"));
    options.AddPolicy("IsCoach", policy => policy.RequireRole("coach"));
    options.AddPolicy("IsCoachOrClubManager", policy => policy.RequireRole("coach", "club-manager"));
});
builder.Services.AddDispatchR(_ => { });

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

var app = builder.Build();
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
await kurrentDbProjection.EnableAsync("$by_event_type");

var projections = kurrentDbProjection.ListContinuousAsync();
if (!await projections.AnyAsync(projection => projection.Name == "Teams-Stream"))
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