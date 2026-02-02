using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetMember;

/// <summary>
/// Queries a team member by id.
/// </summary>
[UsedImplicitly]
public record GetMemberQuery(string Team, Guid Id) : IRequest<GetMemberQuery, Task<MemberReadDto?>>, ITeamQuery;