using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using MapsterMapper;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;

[UsedImplicitly]
public class StampCardGetByIdQueryHandler(IStampCardModelLoader stampCardModelLoader, IMapper mapper) 
    : IRequestHandler<StampCardDetailedGetByIdQuery, Task<Result<StampCardReadDetailsDto>>>, 
        IRequestHandler<StampCardGetByIdQuery, Task<Result<StampCardReadDto>>>
{

    public async Task<Result<StampCardReadDetailsDto>> Handle(
        StampCardDetailedGetByIdQuery message, 
        CancellationToken cancellationToken)
    {
        var stampCardAggregate = await stampCardModelLoader.LoadModelAsync(message.Season, message.Team);
        var stampCard = await stampCardAggregate.GetById(message.Id);
        if (stampCard == null) return Result.Fail($"Stempelkarte '{message.Id}' konnte nicht gefunden werden.");
        var stampCardReadDetailDto = mapper.Map<StampCardReadDetailsDto>(stampCard);
        stampCardReadDetailDto.Team = message.Team;
        stampCardReadDetailDto.Season = message.Season;
        return Result.Ok(stampCardReadDetailDto);
    }

    public async Task<Result<StampCardReadDto>> Handle(StampCardGetByIdQuery message, CancellationToken cancellationToken)
    {
        var stampCardAggregate = await stampCardModelLoader.LoadModelAsync(message.Season, message.Team);
        var stampCard = await stampCardAggregate.GetById(message.Id);
        if (stampCard == null) return Result.Fail($"Stempelkarte '{message.Id}' konnte nicht gefunden werden.");
        var stampCardReadDetailDto = mapper.Map<StampCardReadDto>(stampCard);
        stampCardReadDetailDto.Team = message.Team;
        stampCardReadDetailDto.Season = message.Season;
        return Result.Ok(stampCardReadDetailDto);
    }
}