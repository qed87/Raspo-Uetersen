using JetBrains.Annotations;
using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Dtos;
using StampCard.Backend.Model;

namespace StampCard.Backend.Queries.GetStampsQuery;

/// <inheritdoc />
[UsedImplicitly]
public class GetStampQueryHandler(IServiceProvider serviceProvider, ILogger<GetStampQueryHandler> logger) 
    : QueryHandlerBase<GetStampsQuery, List<StampReadDto>>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override async Task<List<StampReadDto>> GetResult(ITeamAggregate model, GetStampsQuery request)
    {
        var result = await model.GetStampsFromStampCardAsync(request.StampCardId);
        return result.Select(stamp => new StampReadDto(stamp.Id, stamp.Reason, stamp.Issuer, stamp.IssuedOn)).ToList();
    }
}