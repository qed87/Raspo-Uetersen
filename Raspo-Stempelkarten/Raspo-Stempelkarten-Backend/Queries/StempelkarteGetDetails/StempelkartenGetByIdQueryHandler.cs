using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using MapsterMapper;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StempelkarteGetDetails;

[UsedImplicitly]
public class StempelkartenGetByIdQueryHandler(IStempelkartenModelLoader stempelkartenModelLoader, IMapper mapper) 
    : IRequestHandler<StempelkartenGetByIdQuery, Task<Result<StempelkartenReadDetailsDto>>>
{

    public async Task<Result<StempelkartenReadDetailsDto>> Handle(
        StempelkartenGetByIdQuery message, 
        CancellationToken cancellationToken)
    {
        var stempelkartenAggregate = await stempelkartenModelLoader.LoadModelAsync(message.Team, message.Season);
        var stempelkarte = stempelkartenAggregate.GetById(message.Id);
        return Result.Ok(mapper.Map<StempelkartenReadDetailsDto>(stempelkarte));
    }
}