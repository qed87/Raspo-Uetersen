using DispatchR.Abstractions.Send;

namespace Raspo_Stempelkarten_Backend.Queries.Teams;

public sealed class GetTeamSeasonQuery : IRequest<GetTeamSeasonQuery, Task<StampCardTeamsAndSeasonsResult>>
{
}