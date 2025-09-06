using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteStamp;

[UsedImplicitly]
public class StempelkarteStampCommandHandler(
    IStempelkartenModelStorage storage,
    IHttpContextAccessor contextAccessor, 
    IStempelkartenModelLoader modelLoader,
    IStampCardChangeTracker changeTracker)
    : IRequestHandler<StempelkartenStampCommand, Task<Result<StempelkartenStampResponse>>>
{
    public async Task<Result<StempelkartenStampResponse>> Handle(StempelkartenStampCommand message, CancellationToken cancellationToken)
    {
        var model = await modelLoader.LoadModelAsync(
            message.Team, message.Season);
        var stampResult = await model.Stamp(message.StempelkartenId, contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo", message.Reason);
        var changes = changeTracker.GetChanges().ToList();
        if (stampResult.IsFailed)
        {
            return Result.Fail<StempelkartenStampResponse>(stampResult.Errors);
        }
        
        if (!changes.Any())
        {
            return Result.Fail<StempelkartenStampResponse>("Stempelkarte konnte nicht gestempelt werden!");
        }
        
        var result = await storage.StoreAsync(message.Team, message.Season, model.ConcurrencyToken, 
            changes, cancellationToken);

        return Result.Ok(new StempelkartenStampResponse(stampResult.Value.Id, (ulong) result.Value));
    }
}