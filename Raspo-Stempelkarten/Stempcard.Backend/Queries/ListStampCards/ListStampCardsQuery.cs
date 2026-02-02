using DispatchR.Abstractions.Stream;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.ListStampCards;

/// <summary>
/// List all stamp cards of the team.
/// </summary>
public record ListStampCardsQuery(string Team) 
    : IStreamRequest<ListStampCardsQuery, StampCardReadDto>;