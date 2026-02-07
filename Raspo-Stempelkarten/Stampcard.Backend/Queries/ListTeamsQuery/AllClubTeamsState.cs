using JetBrains.Annotations;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.ListTeamsQuery;

/// <summary>
/// Represents the state of the all-club-teams projection.
/// </summary>
[UsedImplicitly]
public class AllClubTeamsState
{
    /// <summary>
    /// Returns all active teams of the application.
    /// </summary>
    public List<TeamReadDto>? Teams { get; set; }
}
