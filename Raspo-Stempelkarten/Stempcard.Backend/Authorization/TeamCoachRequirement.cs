using Microsoft.AspNetCore.Authorization;

namespace Raspo_Stempelkarten_Backend.Authorization;

/// <summary>
/// Team coach requirement.
/// </summary>
public class TeamCoachRequirement : IAuthorizationRequirement
{
}