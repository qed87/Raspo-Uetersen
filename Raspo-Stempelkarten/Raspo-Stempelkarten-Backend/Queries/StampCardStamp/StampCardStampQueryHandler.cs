using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using MapsterMapper;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardStamp;

[UsedImplicitly]
public class StampCardStampQueryHandler(IStampCardModelLoader stampCardModelLoader, IMapper mapper) 
    : IRequestHandler<StampCardStampGetByIdQuery, Task<Result<StampReadDto>>>,
        IRequestHandler<StampCardStampListQuery, Task<Result<IEnumerable<StampReadDto>>>>
{
    public async Task<Result<StampReadDto>> Handle(StampCardStampGetByIdQuery message, CancellationToken cancellationToken)
    {
        var stampCardAggregate = await stampCardModelLoader.LoadModelAsync(message.Season, message.Team);
        var stamp = await stampCardAggregate.GetStampById(message.StampCardId, message.Id);
        if (stamp == null) return Result.Fail($"Stempelkarte '{message.Id}' konnte nicht gefunden werden.");
        var stampReadDto = mapper.Map<StampReadDto>(stamp);
        return Result.Ok(stampReadDto);
    }

    public async Task<Result<IEnumerable<StampReadDto>>> Handle(StampCardStampListQuery message, CancellationToken cancellationToken)
    {
        var stampCardAggregate = await stampCardModelLoader.LoadModelAsync(message.Season, message.Team);
        var stamps = await stampCardAggregate.GetStamps(message.Id);
        var stampReadDtos = mapper.Map<IEnumerable<StampReadDto>>(stamps);
        return Result.Ok(stampReadDtos);
    }
}