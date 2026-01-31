using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.ListStampCards;

/// <inheritdoc />
public class ListStampCardQueryHandler(IServiceProvider serviceProvider) 
    : IStreamRequestHandler<ListStampCardsQuery, StampCardReadDto>
{
    /// <inheritdoc />
    public async IAsyncEnumerable<StampCardReadDto> Handle(ListStampCardsQuery request, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.StreamId);
        foreach (var stampCard in model.Cards)
        {
            var stampCardReadDto = new StampCardReadDto
            {
                Id = stampCard.Id,
                AccountingYear = stampCard.AccountingYear,
                IssuedTo = stampCard.PlayerId,
                IssuedAt = stampCard.IssuedDate
            };
            
            yield return stampCardReadDto;
        }
    }
}