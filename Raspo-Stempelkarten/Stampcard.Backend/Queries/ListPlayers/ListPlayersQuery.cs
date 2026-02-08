using DispatchR.Abstractions.Stream;
using JetBrains.Annotations;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.ListPlayers;

/// <summary>
/// The players of the team.
/// </summary>
[UsedImplicitly]
public record ListPlayersQuery(string Team) : IStreamRequest<ListPlayersQuery, PlayerReadDto>;