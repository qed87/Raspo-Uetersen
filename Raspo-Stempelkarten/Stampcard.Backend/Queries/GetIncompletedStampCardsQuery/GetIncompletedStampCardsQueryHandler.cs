using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Queries.GetIncompletedStampCardsQuery;

/// <inheritdoc />
public class GetIncompletedStampCardsQueryHandler(IServiceProvider serviceProvider, ILogger<GetIncompletedStampCardsQueryHandler> logger) 
    : QueryHandlerBase<GetIncompletedStampCardsQuery, List<StampCardReadDetailsDto>>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override Task<List<StampCardReadDetailsDto>> GetResult(ITeamAggregate model, GetIncompletedStampCardsQuery request)
    {
        var stampCardsResponse = model.GetIncompleteStampCards(request.AccountingYear, request.NumberOfRequiredStamps);
        if (!stampCardsResponse.IsSuccess || stampCardsResponse.ValueOrDefault is null) return Task.FromResult<List<StampCardReadDetailsDto>>([]);
        return Task.FromResult(stampCardsResponse.ValueOrDefault.Select(card =>
        {
            return new StampCardReadDetailsDto(card.Id, card.PlayerId, card.AccountingYear, card.Issuer, card.IssuedOn)
            {
                Stamps = card.Stamps.Select(stamp => new StampReadDto(stamp.Id, stamp.Reason, stamp.Issuer, stamp.IssuedOn)).ToList()
            };
        }).ToList());
    }
}