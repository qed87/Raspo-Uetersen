using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StampCardDelete;

[UsedImplicitly]
public class StampCardDeleteCommandHandler(
    IStampCardModelStorage storage,
    IHttpContextAccessor contextAccessor, 
    IStampCardModelLoader modelLoader,
    IStampCardChangeTracker changeTracker) 
    : IRequestHandler<StampCardDeleteCommand, Task<Result>>
{
    public async Task<Result> Handle(StampCardDeleteCommand message, CancellationToken cancellationToken)
    {
        var model = await modelLoader.LoadModelAsync(message.Season, message.Team);
        var result = await model.RemoveStampCard(
            message.Id,
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo");
        if (result.IsFailed)
        {
            return Result.Fail(result.Errors);
        }
        var changes = changeTracker.GetChanges().ToList();
        if (!changes.Any())
        {
            return Result.Fail("Stempelkarte ist konnte nicht gelöscht werden!");
        }

        await storage.StoreAsync(message.Season, message.Team, 
            model.ConcurrencyToken, changes, cancellationToken);

        return Result.Ok();
    }
}