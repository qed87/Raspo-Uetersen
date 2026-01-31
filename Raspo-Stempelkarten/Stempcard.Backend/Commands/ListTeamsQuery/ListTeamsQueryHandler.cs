using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.ListTeamsQuery;

/// <inheritdoc />
public class ListTeamsQueryHandler(ITeamService teamService) 
    : IRequestHandler<ListTeamsQuery, Task<List<TeamReadDto>>>
{
    /// <inheritdoc />
    public Task<List<TeamReadDto>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        return teamService.ListTeamsAsync(cancellationToken)!;
    }
}