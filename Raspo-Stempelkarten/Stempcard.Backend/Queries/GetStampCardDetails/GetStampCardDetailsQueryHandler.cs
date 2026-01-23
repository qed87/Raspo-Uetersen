using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.GetStampCard;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCardDetails;

public class GetStampCardDetailsQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>
{
    public async Task<StampCardReadDetailsDto?> Handle(GetStampCardDetailsQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.StreamId);
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return null;
        var stampCardReadDto = new StampCardReadDetailsDto
        {
            Id = stampCard.Id,
            AccountingYear = stampCard.AccountingYear,
            IssuedTo = stampCard.IssuedTo,
            IssuedAt = stampCard.IssuedAt,
            Stamps = stampCard.Stamps.Select(stamp => 
                new StampReadDto
                {
                    Id = stamp.Id,
                    Reason = stamp.Reason
                }).ToList()
        };
        
        return stampCardReadDto;
    }
}