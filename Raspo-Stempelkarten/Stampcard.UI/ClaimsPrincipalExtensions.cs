using System.Security.Claims;
using Stampcard.UI.Clients;

namespace Stampcard.UI;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The email of the user, which the backend identifies coaches by. The preferred username may differ from it.
    /// </summary>
    public static string? Email(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Indicates whether the user coaches the team. The backend stores coaches lower-cased.
    /// </summary>
    public static bool IsCoachOf(this ClaimsPrincipal user, TeamReadDto team)
    {
        var email = user.Email();
        return email is not null && team.Coaches.Contains(email, StringComparer.OrdinalIgnoreCase);
    }
}
