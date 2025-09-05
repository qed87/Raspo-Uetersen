using FluentResults;
using JetBrains.Annotations;
using LiteBus.Commands.Abstractions;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteStamp;

[UsedImplicitly]
public class StempelkarteStampCommandHandler(
    IStempelkartenModelStorage storage,
    IHttpContextAccessor contextAccessor, 
    IStempelkartenModelLoader modelLoader,
    StampCardChangeTracker stampCardChangeTracker)
    : ICommandHandler<StempelkartenStampCommand, Result<StempelkartenStampResponse>>
{
    public async Task<Result<StempelkartenStampResponse>> HandleAsync(
        StempelkartenStampCommand message, 
        CancellationToken cancellationToken = new())
    {
        var model = await modelLoader.LoadModelAsync(
            message.Team, message.Season);
        stampCardChangeTracker.Enable();
        var stampResult = await model.Stamp(message.StempelkartenId, contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo", message.Reason);
        var changes = stampCardChangeTracker.GetChanges().ToList();
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