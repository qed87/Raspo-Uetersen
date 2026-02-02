using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.ListMembers;

/// <summary>
/// The members of the the team.
/// </summary>
[UsedImplicitly]
public record ListMembersQuery(string Team) : IStreamRequest<ListMembersQuery, MemberReadDto>;