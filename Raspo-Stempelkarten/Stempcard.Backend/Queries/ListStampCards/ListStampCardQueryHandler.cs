using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Queries.ListStampCards;

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
        var model = await modelLoader.LoadModelAsync(request.Team);
        foreach (var stampCard in model.Cards)
        {
            var stampCardReadDto = new StampCardReadDto(
                stampCard.Id, 
                stampCard.AccountingYear, 
                stampCard.Issuer,
                stampCard.IssuedOn);
            yield return stampCardReadDto;
        }
    }
}