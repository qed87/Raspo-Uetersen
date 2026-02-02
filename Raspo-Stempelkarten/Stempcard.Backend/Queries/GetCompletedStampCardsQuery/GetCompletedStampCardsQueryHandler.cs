using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;

/// <inheritdoc />
public class GetCompletedStampCardsQueryHandler(IServiceProvider serviceProvider) 
    : QueryHandlerBase<GetCompletedStampCardsQuery, List<StampCardReadDetailsDto>?>(serviceProvider)
{
    /// <inheritdoc />
    protected override Task<List<StampCardReadDetailsDto>?> GetResult(ITeamAggregate model, GetCompletedStampCardsQuery request)
    {
        var stampCardsResponse = model.GetCompleteStampCards(request.AccountingYear, request.NumberOfRequiredStamps);
        if (!stampCardsResponse.IsSuccess || stampCardsResponse.ValueOrDefault is null) return null!;
        return Task.FromResult(stampCardsResponse.ValueOrDefault?.Select(card =>
        {
            return new StampCardReadDetailsDto(card.Id, card.MemberId, card.AccountingYear, card.Issuer, card.IssuedOn)
            {
                Stamps = card.Stamps.Select(stamp => new StampReadDto(stamp.Id, stamp.Reason)).ToList()
            };
        }).ToList());
    }
}