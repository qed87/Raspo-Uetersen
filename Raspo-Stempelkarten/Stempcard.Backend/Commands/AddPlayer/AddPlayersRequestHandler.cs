using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.AddPlayer;

[UsedImplicitly]
public class AddPlayersRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<AddPlayersRequest, Task<Result<AddPlayersResponse>>>
{
    public async Task<Result<AddPlayersResponse>> Handle(AddPlayersRequest request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = model.AddPlayer(request.FirstName, request.Surname, request.Birthdate);
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IStampModelStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new AddPlayersResponse { Id = result.Value });
    }
}