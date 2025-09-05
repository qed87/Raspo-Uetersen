using FluentResults;
using JetBrains.Annotations;
using LiteBus.Queries.Abstractions;
using MapsterMapper;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StempelkarteGetDetails;

[UsedImplicitly]
public class StempelkartenGetByIdQueryHandler(IStempelkartenModelLoader stempelkartenModelLoader, IMapper mapper) 
    : IQueryHandler<StempelkartenGetByIdQuery, Result<StempelkartenReadDetailsDto>>
{
    public async Task<Result<StempelkartenReadDetailsDto>> HandleAsync(StempelkartenGetByIdQuery message, 
        CancellationToken cancellationToken = default)
    {
        var stempelkartenAggregate = await stempelkartenModelLoader.LoadModelAsync(message.Team, message.Season);
        var stempelkarte = stempelkartenAggregate.GetById(message.Id);
        return stempelkarte == null ? 
            Result.Fail("Stempelkarte konnte nicht gefunden werden.") : 
            Result.Ok(mapper.Map<StempelkartenReadDetailsDto>(stempelkarte));
    }
}