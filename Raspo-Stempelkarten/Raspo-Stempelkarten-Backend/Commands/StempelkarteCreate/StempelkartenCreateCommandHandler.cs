using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.StempelkarteCreate;

[UsedImplicitly]
public class StempelkartenCreateCommandHandler(
    IStampCardChangeTracker changeTracker,
    IStempelkartenModelLoader modelLoader,
    IStempelkartenModelStorage modelStorage, 
    IHttpContextAccessor contextAccessor) 
    : IRequestHandler<StempelkartenCreateCommand, Task<Result<StempelkartenCreateResponse>>>
{
    public async Task<Result<StempelkartenCreateResponse>> Handle(
        StempelkartenCreateCommand message, 
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
 
        return Result.Ok(new StempelkartenCreateResponse(
            stempelkarteResult.Value.Id, 
            (ulong) result.Value));
    }
}