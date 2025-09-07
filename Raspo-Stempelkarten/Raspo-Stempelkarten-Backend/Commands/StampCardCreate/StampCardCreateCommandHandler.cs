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
        var model = await modelLoader.LoadModelAsync(
            message.Dto.Team, message.Dto.Season);
        var stempelkarteResult = await model.AddStampCard(
            message.Dto.Recipient, 
            contextAccessor.HttpContext?.User.Identity?.Name ?? "dbo",
            message.Dto.MinStamps, 
            message.Dto.MaxStamps);
        var changes = changeTracker.GetChanges().ToList();
        if (stempelkarteResult.IsFailed)
        {
            return Result.Fail("Stempelkarte konnte nicht angelegt werden!");
        }
        
        if (!changes.Any())
        {
            return Result.Ok();
        }

        var result = await modelStorage.StoreAsync(message.Dto.Team, message.Dto.Season, 
            model.ConcurrencyToken, changes, cancellationToken);
 
        return Result.Ok(new StampCardCreateResponse(
            stempelkarteResult.Value.Id, 
            (ulong) result.Value));
    }
}