using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;

/// <inheritdoc />
public class DeleteStampCardRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<DeleteStampCard, Task<Result<DeleteStampCardResponse>>>
{
    /// <inheritdoc />
    public async Task<Result<DeleteStampCardResponse>> Handle(DeleteStampCard request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.Team);
        var result = await model.DeleteTeamAsync();
        if (!result.IsSuccess) return result.ToResult();
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok();
    }
}