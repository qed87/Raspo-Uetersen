using DispatchR.Abstractions.Stream;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.ListStampCards;

/// <summary>
/// List all stamp cards of the team.
/// </summary>
public record ListStampCardsQuery(string Team) 
    : IStreamRequest<ListStampCardsQuery, StampCardReadDto>;