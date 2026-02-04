using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Dtos;
using StampCard.Backend.Model;

namespace StampCard.Backend.Queries.GetStampCardDetails;

/// <inheritdoc />
public class GetStampCardDetailsQueryHandler(IServiceProvider serviceProvider, ILogger<GetStampCardDetailsQueryHandler> logger) 
    : QueryHandlerBase<GetStampCardDetailsQuery, StampCardReadDetailsDto?>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override Task<StampCardReadDetailsDto?> GetResult(ITeamAggregate model, GetStampCardDetailsQuery request)
    {
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return Task.FromResult<StampCardReadDetailsDto?>(null);
        var stampCardReadDto = new StampCardReadDetailsDto(stampCard.Id, stampCard.PlayerId, stampCard.AccountingYear,
            stampCard.Issuer, stampCard.IssuedOn) 
        {
            Stamps = stampCard.Stamps.Select(stamp => 
                new StampReadDto(stamp.Id, stamp.Reason, stamp.Issuer, stamp.IssuedOn)).ToList()
        };
        
        return Task.FromResult(stampCardReadDto)!;
    }
}