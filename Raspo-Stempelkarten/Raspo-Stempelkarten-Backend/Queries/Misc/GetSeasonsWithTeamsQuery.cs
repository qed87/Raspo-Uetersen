using DispatchR.Abstractions.Send;

namespace Raspo_Stempelkarten_Backend.Queries.Misc;

public sealed class GetSeasonsWithTeamsQuery : IRequest<GetSeasonsWithTeamsQuery, Task<SeasonsResult>>
{
}