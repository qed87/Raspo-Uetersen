using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;

[UsedImplicitly]
public class StempelkartenDeleteCommandHandler(
    IStempelkartenModelStorage storage,
    IHttpContextAccessor contextAccessor, 
    IStempelkartenModelLoader modelLoader,
    IStampCardChangeTracker changeTracker) 
    : IRequestHandler<StempelkartenDeleteCommand, Task<Result>>
{
    public async Task<Result> Handle(StempelkartenDeleteCommand message, CancellationToken cancellationToken)
    {
        var model = await modelLoader.LoadModelAsync(
            message.Team, message.Season);
        var result = await model.RemoveStampCard(
            message.Id,
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo");
        if (result.IsFailed)
        {
            return Result.Fail("Stempelkarte konnte nicht gelöscht werden!");
        }
        var changes = changeTracker.GetChanges().ToList();
        if (!changes.Any())
        {
            return Result.Fail("Stempelkarte konnte nicht gelöscht werden!");
        }

        await storage.StoreAsync(message.Team, message.Season, model.ConcurrencyToken, 
            changes, cancellationToken);

        return Result.Ok();
    }
}