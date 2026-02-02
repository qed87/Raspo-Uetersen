using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;

/// <inheritdoc />
public class GetIncompletedStampCardsQueryHandler(IServiceProvider serviceProvider) 
    : QueryHandlerBase<GetIncompletedStampCardsQuery, List<StampCardReadDetailsDto>?>(serviceProvider)
{
    /// <inheritdoc />
    protected override Task<List<StampCardReadDetailsDto>?> GetResult(ITeamAggregate model, GetIncompletedStampCardsQuery request)
    {
        var stampCardsResponse = model.GetIncompleteStampCards(request.AccountingYear, request.NumberOfRequiredStamps);
        if (!stampCardsResponse.IsSuccess || stampCardsResponse.ValueOrDefault is null) return null!;
        return Task.FromResult(stampCardsResponse.ValueOrDefault.Select(card =>
        {
            return new StampCardReadDetailsDto(card.Id, card.MemberId, card.AccountingYear, card.Issuer, card.IssuedOn)
            {
                Stamps = card.Stamps.Select(stamp => new StampReadDto(stamp.Id, stamp.Reason)).ToList()
            };
        }).ToList())!;
    }
}