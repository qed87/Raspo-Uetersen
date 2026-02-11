using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Services.Interfaces;

/// <summary>
/// Team service for reading all available teams.
/// </summary>
public interface ITeamService
{
    /// <summary>
    /// Gets all available teams.
    /// </summary>
    Task<List<TeamReadDto>> ListTeamsAsync(CancellationToken cancellationToken = default);
}