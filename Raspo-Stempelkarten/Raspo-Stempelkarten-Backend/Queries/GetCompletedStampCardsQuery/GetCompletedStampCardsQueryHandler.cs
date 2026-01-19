using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;

public class GetCompletedStampCardsQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetCompletedStampCardsQuery, Task<List<StampCardReadDetailsDto>?>>
{
    public async Task<List<StampCardReadDetailsDto>?> Handle(GetCompletedStampCardsQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var stampCardsResponse = model.GetCompleteStampCards(request.AccountingYear, request.NumberOfRequiredStamps);
        if (!stampCardsResponse.IsSuccess) return null;
        return stampCardsResponse.ValueOrDefault?.Select(card =>
        {
            return new StampCardReadDetailsDto
            {
                Id = card.Id,
                AccountingYear = card.AccountingYear,
                IssuedAt = card.IssuedAt,
                IssuedTo = card.IssuedTo,
                Stamps = card.Stamps.Select(stamp => new StampReadDto()
                {
                    Id = stamp.Id,
                    Reason = stamp.Reason
                }).ToList()
            };
        }).ToList();
    }
}