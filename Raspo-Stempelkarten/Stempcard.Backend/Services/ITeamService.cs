using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Services;

/// <summary>
/// Team service for reading all available teams.
/// </summary>
public interface ITeamService
{
    /// <summary>
    /// Gets all available teams.
    /// </summary>
    Task<List<TeamReadDto>?> ListTeamsAsync(CancellationToken cancellationToken = default);
}