using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

/// <summary>
/// Base command handler for application commands.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class CommandHandlerBase<TCommand, TResult>(IServiceProvider serviceProvider) 
    : IRequestHandler<TCommand, Task<Result<TResult>>> 
    where TCommand : class, ITeamCommand
{
    /// <inheritdoc />
    public async Task<Result<TResult>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var teamModelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await teamModelLoader.LoadModelAsync(request.Team);
        var r1 = await BeforeCommandExecutionAsync(model, request, serviceProvider);
        if (!r1.IsSuccess) return r1.ToResult<TResult>();
        var r2 = await ApplyCommandToModel(request, model);
        if (!r2.IsSuccess) return r2;
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return r2;
    }

    /// <summary>
    /// Validates the model before executing the model command.
    /// </summary>
    protected virtual Task<Result> BeforeCommandExecutionAsync(ITeamAggregate teamModel, TCommand command, IServiceProvider services)
    {
        return Task.FromResult(Result.Ok());
    }

    /// <summary>
    /// Applies the given command to the model.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="model">The model where to apply the command.</param>
    /// <returns></returns>
    protected abstract Task<Result<TResult>> ApplyCommandToModel(TCommand command, ITeamAggregate model);
}