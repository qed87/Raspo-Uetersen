using DispatchR.Abstractions.Send;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.AddCoach;

/// <inheritdoc />
[UsedImplicitly]
public class AddCoachRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<AddCoach, Task<Result>>
{
    /// <inheritdoc />
    public async Task<Result> Handle(AddCoach request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = await model.AddCoach(request.Email, request.IssuedBy);
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return result;
    }
}