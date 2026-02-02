using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

/// <summary>
/// Represents the state of the all-club-teams projection.
/// </summary>
public class AllClubTeamsState
{
    /// <summary>
    /// Returns all active teams of the application.
    /// </summary>
    public List<TeamReadDto>? Teams { get; set; }
}
