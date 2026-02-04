using FluentResults;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Dtos;
using StampCard.Backend.Model;

namespace StampCard.Backend.Queries.GetCompletedStampCardsQuery;

/// <inheritdoc />
public class GetCompletedStampCardsQueryHandler(IServiceProvider serviceProvider, ILogger<GetCompletedStampCardsQueryHandler> logger) 
    : QueryHandlerBase<GetCompletedStampCardsQuery, List<StampCardReadDetailsDto>>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override Task<List<StampCardReadDetailsDto>> GetResult(ITeamAggregate model, GetCompletedStampCardsQuery request)
    {
        var stampCardsResponse = model.GetCompleteStampCards(request.AccountingYear, request.NumberOfRequiredStamps);
        if (!stampCardsResponse.IsSuccess || stampCardsResponse.ValueOrDefault is null) 
            return Task.FromResult<List<StampCardReadDetailsDto>>([]);
        return Task.FromResult(stampCardsResponse.ValueOrDefault!.Select(card =>
        {
            return new StampCardReadDetailsDto(card.Id, card.PlayerId, card.AccountingYear, card.Issuer, card.IssuedOn)
            {
                Stamps = card.Stamps.Select(stamp => new StampReadDto(stamp.Id, stamp.Reason, stamp.Issuer, stamp.IssuedOn)).ToList()
            };
        }).ToList());
    }
}