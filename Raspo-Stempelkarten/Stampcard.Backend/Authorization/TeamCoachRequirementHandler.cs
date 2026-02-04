using Microsoft.AspNetCore.Authorization;
using StampCard.Backend.Model;

namespace StampCard.Backend.Authorization;

/// <summary>
/// AuthorizationHandler for team coach requirement.
/// </summary>
public class TeamCoachRequirementHandler : AuthorizationHandler<TeamCoachRequirement>
{
    /// <inheritdoc />
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TeamCoachRequirement requirement)
    {
        if (context.Resource is not ITeamAggregate teamAggregate)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var coaches = teamAggregate.Coaches.Select(coach => coach.Email).ToList();
        var userName = context.User.Identity!.Name!;
        if (!coaches.Contains(userName))
        {
            context.Fail(new AuthorizationFailureReason(this, "Nur ein Coach darf auf die Resource zugreifen."));
            return Task.CompletedTask;
        }
        
        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}