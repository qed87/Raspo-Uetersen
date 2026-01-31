using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.AddPlayer;

/// <inheritdoc />
[UsedImplicitly]
public class AddPlayersRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<AddPlayersRequest, Task<Result<Guid>>>
{
    /// <inheritdoc />
    public async Task<Result<Guid>> Handle(AddPlayersRequest request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = await model.AddPlayerAsync(request.FirstName, request.LastName, request.Birthdate, request.Birthplace);
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(result.Value);
    }
}