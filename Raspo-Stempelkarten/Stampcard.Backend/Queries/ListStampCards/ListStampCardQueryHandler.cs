using System.Runtime.CompilerServices;
using DispatchR.Abstractions.Stream;
using StampCard.Backend.Dtos;
using StampCard.Backend.Services;

namespace StampCard.Backend.Queries.ListStampCards;

/// <inheritdoc />
public class ListStampCardQueryHandler(IServiceProvider serviceProvider, ILogger<ListStampCardQueryHandler> logger) 
    : IStreamRequestHandler<ListStampCardsQuery, StampCardReadDto>
{
    /// <inheritdoc />
    public async IAsyncEnumerable<StampCardReadDto> Handle(ListStampCardsQuery request, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        logger.LogTrace("Loading team from database...");
        var model = await modelLoader.LoadModelAsync(request.Team);
        if (model is null) yield break;
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