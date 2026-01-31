using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.GetCompletedStampCardsQuery;

/// <inheritdoc />
public class GetCompletedStampCardsQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetCompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>
{
    /// <inheritdoc />
    public async Task<List<StampCardReadDetailsDto>?> Handle(GetCompletedStampCardsQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.Team);
        var stampCardsResponse = model.GetCompleteStampCards(request.AccountingYear, request.NumberOfRequiredStamps);
        if (!stampCardsResponse.IsSuccess) return null;
        return stampCardsResponse.ValueOrDefault?.Select(card =>
        {
            return new StampCardReadDetailsDto
            {
                Id = card.Id,
                AccountingYear = card.AccountingYear,
                IssuedAt = card.IssuedDate,
                PlayerId = card.PlayerId,
                Stamps = card.Stamps.Select(stamp => new StampReadDto()
                {
                    Id = stamp.Id,
                    Reason = stamp.Reason
                }).ToList()
            };
        }).ToList();
    }
}