using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.StampStampCard;

/// <inheritdoc />
public class StampStampCardRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<StampStampCard, Task<Result<StampStampCardResponse>>>
{
    /// <inheritdoc />
    public async Task<Result<StampStampCardResponse>> Handle(StampStampCard request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = await model.StampStampCardAsync(request.StampCardId, request.Reason);
        if (!result.IsSuccess) return result.ToResult();
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new StampStampCardResponse { Id = result.Value });
    }
}