using System.Security.Claims;
using Stampcard.UI.Clients;

namespace Stampcard.UI;

public static class ClaimsPrincipalExtensions
{
    public const string ManagerRole = "manager";

    public const string AccountingRole = "accounting";

    /// <summary>
    /// Users of the accounting only see the accounting page, unless they are managers as well.
    /// </summary>
    public static bool IsRestrictedToAccounting(this ClaimsPrincipal user)
    {
        return user.IsInRole(AccountingRole) && !user.IsInRole(ManagerRole);
    }

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
