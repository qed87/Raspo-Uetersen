using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.EraseStamp;

/// <inheritdoc />
public class EraseStampRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<EraseStamp, Task<Result<EraseStampResponse>>>
{
    /// <inheritdoc />
    public async Task<Result<EraseStampResponse>> Handle(EraseStamp request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.Team);
        var result = await model.EraseStampAsync(request.StampCardId, request.StampId);
        if (!result.IsSuccess) return result.ToResult();
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new EraseStampResponse { Id = result.Value });
    }
}