using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Commands.StampCardCreate;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardUpdate;

[UsedImplicitly]
public class StampCardUpdateCommandHandler(
    IStampCardChangeTracker changeTracker,
    IStampCardModelLoader modelLoader,
    IStampCardModelStorage modelStorage, 
    IHttpContextAccessor contextAccessor) 
    : IRequestHandler<StampCardUpdateCommand, Task<Result<StampCardUpdateResponse>>>
{
    public async Task<Result<StampCardUpdateResponse>> Handle(StampCardUpdateCommand message, CancellationToken cancellationToken)
    {
        var model = await modelLoader.LoadModelAsync(message.Dto.Season, message.Dto.Team);
        var updateStampCardResult = await model.Update(
            message.Dto.Id,
            message.Dto.Recipient, 
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo",
            message.Dto.MinStamps, 
            message.Dto.MaxStamps,
            message.Dto.Owners);
        var changes = changeTracker.GetChanges().ToList();
        if (updateStampCardResult.IsFailed)
        {
            return Result.Fail(updateStampCardResult.Errors);
        }
        
        if (!changes.Any())
        {
            return Result.Ok();
        }
        
        var result = await modelStorage.StoreAsync(message.Dto.Season, 
            message.Dto.Team, model.ConcurrencyToken, changes, cancellationToken);
        
        return Result.Ok(new StampCardUpdateResponse(
            updateStampCardResult.Value.Id, 
            (ulong) result.Value));
    }
}