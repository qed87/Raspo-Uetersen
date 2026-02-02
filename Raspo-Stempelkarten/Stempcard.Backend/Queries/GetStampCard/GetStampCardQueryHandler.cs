using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCard;

/// <inheritdoc />
public class GetStampCardQueryHandler(IServiceProvider serviceProvider) 
    : QueryHandlerBase<GetStampCardQuery, StampCardReadDto?>(serviceProvider)
{
    /// <inheritdoc />
    protected override Task<StampCardReadDto?> GetResult(ITeamAggregate model, GetStampCardQuery request)
    {
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return null;
        var stampCardReadDto = new StampCardReadDto(stampCard.Id, stampCard.AccountingYear, stampCard.Issuer,
            stampCard.IssuedOn);
        return Task.FromResult(stampCardReadDto)!;
    }
}