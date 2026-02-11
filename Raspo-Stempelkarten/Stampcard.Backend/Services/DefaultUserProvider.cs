using StampCard.Backend.Services.Interfaces;

namespace StampCard.Backend.Services;

/// <summary>
/// The default user provider.
/// </summary>
public class DefaultUserProvider(IHttpContextAccessor httpContextAccessor) : IUserProvider
{
    /// <summary>
    /// Returns the username of the current user.
    /// </summary>
    /// <returns></returns>
    public string? GetUserName()
    {
        return httpContextAccessor.HttpContext?.User.Identity?.Name;
    }
}