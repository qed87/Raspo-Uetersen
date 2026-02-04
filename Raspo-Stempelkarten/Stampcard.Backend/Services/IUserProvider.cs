namespace StampCard.Backend.Services;

/// <summary>
/// User provider service.
/// </summary>
public interface IUserProvider
{
    /// <summary>
    /// Returns the username.
    /// </summary>
    string? GetUserName();
}