using Microsoft.AspNetCore.Authorization;

namespace StampCard.Backend.Authorization;

/// <summary>
/// Team coach requirement.
/// </summary>
public class TeamCoachRequirement : IAuthorizationRequirement
{
}