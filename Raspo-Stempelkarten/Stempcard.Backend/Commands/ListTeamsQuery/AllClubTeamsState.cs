using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.ListTeamsQuery;

/// <summary>
/// Represents the state of the all-club-teams projection.
/// </summary>
public class AllClubTeamsState
{
    public List<TeamReadDto> Teams { get; set; }
}
