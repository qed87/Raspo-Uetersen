namespace Raspo_Stempelkarten_Backend.Services;

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