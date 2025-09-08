using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using MapsterMapper;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;

namespace Raspo_Stempelkarten_Backend.Queries.StampCardList;

[UsedImplicitly]
public class StampCardListQueryHandler(IStampCardModelLoader stampCardModelLoader, IMapper mapper) 
    : IRequestHandler<StampCardListQuery, Task<Result<IEnumerable<StampCardReadDto>>>>
{

    public async Task<Result<IEnumerable<StampCardReadDto>>> Handle(
        StampCardListQuery message, 
        CancellationToken cancellationToken)
    {
        var stampCardAggregate = await stampCardModelLoader.LoadModelAsync(message.Team, message.Season);
        var stampCards = await stampCardAggregate.List();
        var stampCardReadDetailsDto = mapper.Map<IEnumerable<StampCardReadDto>>(stampCards).ToList();
        foreach (var stampCardReadDto in stampCardReadDetailsDto)
        {
            stampCardReadDto.Team = message.Team;
            stampCardReadDto.Season = message.Season;
        }
        return Result.Ok<IEnumerable<StampCardReadDto>>(stampCardReadDetailsDto);
    }
}