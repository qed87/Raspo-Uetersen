using FluentResults;
using JetBrains.Annotations;
using LiteBus.Commands.Abstractions;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;

[UsedImplicitly]
public class StempelkartenDeleteCommandHandler(
    IStempelkartenModelStorage storage,
    IHttpContextAccessor contextAccessor, 
    IStempelkartenModelLoader modelLoader,
    StampCardChangeTracker stampCardChangeTracker) 
    : ICommandHandler<StempelkartenDeleteCommand, Result>
{

    public async Task<Result> HandleAsync(
        StempelkartenDeleteCommand message, 
        CancellationToken cancellationToken = default)
    {
        var model = await modelLoader.LoadModelAsync(
            message.Team, message.Season);
        stampCardChangeTracker.Enable();
        var result = await model.RemoveStampCard(
            message.Id,
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo");
        if (result.IsFailed)
        {
            return Result.Fail("Stempelkarte konnte nicht gelöscht werden!");
        }
        var changes = stampCardChangeTracker.GetChanges().ToList();
        if (!changes.Any())
        {
            return Result.Fail("Stempelkarte konnte nicht gelöscht werden!");
        }

        await storage.StoreAsync(message.Team, message.Season, model.ConcurrencyToken, 
            changes, cancellationToken);

        return Result.Ok();
    }
}