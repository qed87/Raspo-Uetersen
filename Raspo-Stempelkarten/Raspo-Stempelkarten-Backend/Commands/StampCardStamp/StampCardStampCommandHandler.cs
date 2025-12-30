using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardStamp;

[UsedImplicitly]
public class StampCardStampCommandHandler(
    IStampCardModelStorage storage,
    IHttpContextAccessor contextAccessor, 
    IStampCardModelLoader modelLoader,
    IStampCardChangeTracker changeTracker)
    : IRequestHandler<StampCardStampCommand, Task<Result<StampCardStampResponse>>>
{
    public async Task<Result<StampCardStampResponse>> Handle(StampCardStampCommand message, CancellationToken cancellationToken)
    {
        var model = await modelLoader.LoadModelAsync(message.Season, message.Team);
        var stampResult = await model.Stamp(message.StampCardId, contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo", message.Reason);
        var changes = changeTracker.GetChanges().ToList();
        if (stampResult.IsFailed)
        {
            return Result.Fail<StampCardStampResponse>(stampResult.Errors);
        }
        
        if (!changes.Any())
        {
            return Result.Fail<StampCardStampResponse>("Stempelkarte konnte nicht gestempelt werden!");
        }
        
        var result = await storage.StoreAsync(message.Season, message.Team, 
            model.ConcurrencyToken, changes, cancellationToken);

        return Result.Ok(new StampCardStampResponse(stampResult.Value.Id, (ulong) result.Value));
    }
}