namespace StampCard.Backend.Services.Interfaces;

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