using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.DeletePlayer;

/// <inheritdoc />
[UsedImplicitly]
public class DeletePlayerRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<DeletePlayerRequest, Task<Result<DeletePlayerResponse>>>
{
    /// <inheritdoc />
    public async Task<Result<DeletePlayerResponse>> Handle(DeletePlayerRequest request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.Team);
        var result = await model.DeletePlayerAsync(request.Id);
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new DeletePlayerResponse { Id = result.Value });
    }
}