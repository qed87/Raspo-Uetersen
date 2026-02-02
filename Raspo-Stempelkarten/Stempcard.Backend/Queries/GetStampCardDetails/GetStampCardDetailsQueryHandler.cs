using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCardDetails;

/// <inheritdoc />
public class GetStampCardDetailsQueryHandler(IServiceProvider serviceProvider) 
    : QueryHandlerBase<GetStampCardDetailsQuery, StampCardReadDetailsDto?>(serviceProvider)
{
    /// <inheritdoc />
    protected override Task<StampCardReadDetailsDto?> GetResult(ITeamAggregate model, GetStampCardDetailsQuery request)
    {
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return null!;
        var stampCardReadDto = new StampCardReadDetailsDto(stampCard.Id, stampCard.MemberId, stampCard.AccountingYear,
            stampCard.Issuer, stampCard.IssuedOn) 
        {
            Stamps = stampCard.Stamps.Select(stamp => 
                new StampReadDto(stamp.Id, stamp.Reason)).ToList()
        };
        
        return Task.FromResult(stampCardReadDto)!;
    }
}