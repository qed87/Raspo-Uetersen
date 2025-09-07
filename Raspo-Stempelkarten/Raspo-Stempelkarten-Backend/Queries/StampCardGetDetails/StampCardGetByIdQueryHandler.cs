using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using MapsterMapper;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardGetDetails;

[UsedImplicitly]
public class StampCardGetByIdQueryHandler(IStampCardModelLoader stampCardModelLoader, IMapper mapper) 
    : IRequestHandler<StampCardGetByIdQuery, Task<Result<StampCardReadDetailsDto>>>
{

    public async Task<Result<StampCardReadDetailsDto>> Handle(
        StampCardGetByIdQuery message, 
        CancellationToken cancellationToken)
    {
        var stempelkartenAggregate = await stampCardModelLoader.LoadModelAsync(message.Team, message.Season);
        var stempelkarte = await stempelkartenAggregate.GetById(message.Id);
        if (stempelkarte == null) return Result.Fail($"Stempelkarte '{message.Id}' konnte nicht gefunden werden.");
        var stempelkartenReadDetailsDto = mapper.Map<StampCardReadDetailsDto>(stempelkarte);
        stempelkartenReadDetailsDto.Team = message.Team;
        stempelkartenReadDetailsDto.Season = message.Season;
        return Result.Ok(stempelkartenReadDetailsDto);
    }
}