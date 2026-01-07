using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.GetStampCard;

public class GetStampCardQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetStampCardQuery, Task<StampCardReadDto?>>
{
    public async Task<StampCardReadDto?> Handle(GetStampCardQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.StreamId);
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return null;
        var stampCardReadDto = new StampCardReadDto
        {
            Id = stampCard.Id,
            AccountingYear = stampCard.AccountingYear,
            IssuedTo = stampCard.IssuedTo,
            IssuedAt = stampCard.IssuedAt,
        };
        
        return stampCardReadDto;
    }
}