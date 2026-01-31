using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.GetStampCardDetails;

/// <inheritdoc />
public class GetStampCardDetailsQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<GetStampCardDetailsQuery, Task<StampCardReadDetailsDto?>>
{
    /// <inheritdoc />
    public async Task<StampCardReadDetailsDto?> Handle(GetStampCardDetailsQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.StreamId);
        var stampCard = model.Cards.SingleOrDefault(card => card.Id == request.Id);
        if (stampCard is null) return null;
        var stampCardReadDto = new StampCardReadDetailsDto
        {
            Id = stampCard.Id,
            AccountingYear = stampCard.AccountingYear,
            PlayerId = stampCard.PlayerId,
            IssuedAt = stampCard.IssuedDate,
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