using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.ListPlayers;

namespace Raspo_Stempelkarten_Backend.Queries.ListStampCards;

public class ListStampCardQueryHandler(IServiceProvider serviceProvider) 
    : IStreamRequestHandler<ListStampCardsQuery, StampCardReadDto>
{
    public async IAsyncEnumerable<StampCardReadDto> Handle(ListStampCardsQuery request, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.StreamId);
        foreach (var stampCard in model.Cards)
        {
            var stampCardReadDto = new StampCardReadDto
            {
                Id = stampCard.Id,
                AccountingYear = stampCard.AccountingYear,
                IssuedTo = stampCard.IssuedTo,
                IssuedAt = stampCard.IssuedAt
            };
            
            yield return stampCardReadDto;
        }
    }
}