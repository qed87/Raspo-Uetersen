using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Dtos;
using StampCard.Backend.Model;

namespace StampCard.Backend.Queries.GetStampCard;

/// <inheritdoc />
public class GetStampCardQueryHandler(IServiceProvider serviceProvider, ILogger<GetStampCardQueryHandler> logger) 
    : QueryHandlerBase<GetStampCardQuery, StampCardReadDto?>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override Task<StampCardReadDto?> GetResult(ITeamAggregate model, GetStampCardQuery request)
    {
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return Task.FromResult<StampCardReadDto?>(null);
        var stampCardReadDto = new StampCardReadDto(stampCard.Id, stampCard.AccountingYear, stampCard.Issuer,
            stampCard.IssuedOn);
        return Task.FromResult(stampCardReadDto)!;
    }
}