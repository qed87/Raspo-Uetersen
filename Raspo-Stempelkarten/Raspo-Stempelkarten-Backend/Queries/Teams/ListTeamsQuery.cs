using DispatchR.Abstractions.Send;

namespace Raspo_Stempelkarten_Backend.Queries.Teams;

public sealed class ListTeamsQuery : IRequest<ListTeamsQuery, Task<IEnumerable<string>>>
{
}