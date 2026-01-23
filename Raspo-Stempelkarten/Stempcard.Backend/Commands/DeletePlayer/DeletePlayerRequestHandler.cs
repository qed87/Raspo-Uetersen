using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.DeletePlayer;

[UsedImplicitly]
public class DeletePlayerRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<DeletePlayerRequest, Task<Result<DeletePlayerResponse>>>
{
    public async Task<Result<DeletePlayerResponse>> Handle(DeletePlayerRequest request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = model.DeletePlayer(request.Id);
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IStampModelStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new DeletePlayerResponse { Id = result.Value });
    }
}