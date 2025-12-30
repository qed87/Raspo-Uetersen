using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardCreate;

[UsedImplicitly]
public class StampCardCreateCommandHandler(
    IStampCardChangeTracker changeTracker,
    IStampCardModelLoader modelLoader,
    IStampCardModelStorage modelStorage, 
    IHttpContextAccessor contextAccessor) 
    : IRequestHandler<StampCardCreateCommand, Task<Result<StampCardCreateResponse>>>
{
    public async Task<Result<StampCardCreateResponse>> Handle(
        StampCardCreateCommand message, 
        CancellationToken cancellationToken)
    {
        var model = await modelLoader.LoadModelAsync(message.Dto.Season, message.Dto.Team);
        var addStampCardResult = await model.AddStampCard(
            message.Dto.Recipient, 
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo",
            message.Dto.MinStamps, 
            message.Dto.MaxStamps,
            message.Dto.Owners);
        var changes = changeTracker.GetChanges().ToList();
        if (addStampCardResult.IsFailed)
        {
            return Result.Fail(addStampCardResult.Errors);
        }
        
        if (!changes.Any())
        {
            return Result.Ok();
        }

        var result = await modelStorage.StoreAsync(message.Dto.Season, 
            message.Dto.Team, model.ConcurrencyToken, changes, cancellationToken);
 
        return Result.Ok(new StampCardCreateResponse(
            addStampCardResult.Value.Id, 
            (ulong) result.Value));
    }
}