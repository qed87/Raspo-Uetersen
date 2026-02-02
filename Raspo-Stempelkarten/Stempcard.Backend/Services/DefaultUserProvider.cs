namespace Raspo_Stempelkarten_Backend.Services;

/// <summary>
/// The default user provider.
/// </summary>
public class DefaultUserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <summary>
    /// Returns the username of the current user.
    /// </summary>
    /// <returns></returns>
    public string? GetUserName()
    {
        return _httpContextAccessor.HttpContext?.User.Identity?.Name;
    }
}