using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.JSInterop.Infrastructure;
using StampCard.Backend.Services.Interfaces;

namespace StampCard.Backend.Authorization;

/// <summary>
/// AuthorizationFilter: Checks whether a user is allowed to access the current resource.
/// </summary>
/// <param name="routeDataKey"></param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TeamCoachRequiredAttribute(string routeDataKey) : Attribute, IAsyncAuthorizationFilter
{
    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity is null || !context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult("Unauthorized");
            return;
        }
        
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<TeamCoachRequiredAttribute>>();
        var teamService = context.HttpContext.RequestServices.GetRequiredService<ITeamService>();
        var teams = await teamService.ListTeamsAsync();
        if (!context.RouteData.Values.TryGetValue(routeDataKey, out var value) || value is not string teamName)
        {
            logger.LogWarning("RouteData contains not a valid '{Key}'.", routeDataKey);
            return;
        }

        var foundTeam = teams.SingleOrDefault(dto => dto.Id == teamName);
        if (foundTeam is null)
        {
            context.Result = new NotFoundObjectResult(teamName);
            return;
        }

        if (foundTeam.Coaches.Contains(context.HttpContext.User.Identity.Name ?? string.Empty)) return;
        context.Result = new UnauthorizedObjectResult("User must be a team coach!");
    }
}