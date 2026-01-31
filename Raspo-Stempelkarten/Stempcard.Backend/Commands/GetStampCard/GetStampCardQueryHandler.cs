using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.GetStampCard;

/// <inheritdoc />
public class GetStampCardQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetStampCardQuery, Task<StampCardReadDto?>>
{
    /// <inheritdoc />
    public async Task<StampCardReadDto?> Handle(GetStampCardQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.StreamId);
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return null;
        var stampCardReadDto = new StampCardReadDto
        {
            Id = stampCard.Id,
            AccountingYear = stampCard.AccountingYear,
            IssuedTo = stampCard.PlayerId,
            IssuedAt = stampCard.IssuedDate,
        };
        
        return stampCardReadDto;
    }
}