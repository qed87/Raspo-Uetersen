using DispatchR.Abstractions.Stream;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.ListStampCards;

public record ListStampCardsQuery(string StreamId) 
    : IStreamRequest<ListStampCardsQuery, StampCardReadDto>;