using DispatchR.Abstractions.Send;
using JetBrains.Annotations;

namespace Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

[UsedImplicitly]
public record ListTeamsQuery : IRequest<ListTeamsQuery, Task<List<string>>>;