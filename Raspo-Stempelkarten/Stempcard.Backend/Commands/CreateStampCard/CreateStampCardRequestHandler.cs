using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.CreateStampCard;

/// <inheritdoc />
public class CreateStampCardRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<CreateStampCard, Task<Result<CreateStampCardResponse>>>
{
    /// <inheritdoc />
    public async Task<Result<CreateStampCardResponse>> Handle(CreateStampCard request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = await model.AddStampCardAsync(request.IssuedTo, request.AccountingYear);
        if (!result.IsSuccess) return result.ToResult();
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new CreateStampCardResponse { Id = result.Value });
    }
}