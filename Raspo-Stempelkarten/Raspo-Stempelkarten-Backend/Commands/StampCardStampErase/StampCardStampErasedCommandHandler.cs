using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Commands.StampCardStamp;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardStampErase;

[UsedImplicitly]
public class StampCardStampErasedCommandHandler(
    IStampCardModelStorage storage,
    IHttpContextAccessor contextAccessor, 
    IStampCardModelLoader modelLoader,
    IStampCardChangeTracker changeTracker)
    : IRequestHandler<StampCardStampErasedCommand, Task<Result<StampCardStampErasedResponse>>>
{
    public async Task<Result<StampCardStampErasedResponse>> Handle(StampCardStampErasedCommand message, CancellationToken cancellationToken)
    {
        var model = await modelLoader.LoadModelAsync(
            message.Team, message.Season);
        var stampResult = await model.EraseStamp(message.StampCardId, message.Id, contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo");
        var changes = changeTracker.GetChanges().ToList();
        if (stampResult.IsFailed)
        {
            return Result.Fail<StampCardStampErasedResponse>(stampResult.Errors);
        }
        
        if (!changes.Any())
        {
            return Result.Fail<StampCardStampErasedResponse>(
                $"Stempel '{message.Id}' konnte nicht von Stempelkarte '{message.StampCardId}' gelöscht werden!");
        }
        
        var result = await storage.StoreAsync(message.Team, message.Season, model.ConcurrencyToken, 
            changes, cancellationToken);

        return Result.Ok(new StampCardStampErasedResponse(stampResult.Value.Id, (ulong) result.Value));
    }
}