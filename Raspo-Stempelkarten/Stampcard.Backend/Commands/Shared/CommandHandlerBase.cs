using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared.Interfaces;
using StampCard.Backend.Model;
using StampCard.Backend.Services;
using StampCard.Backend.Services.Interfaces;

namespace StampCard.Backend.Commands.Shared;

/// <summary>
/// Base command handler for application commands.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class CommandHandlerBase<TCommand, TResult>(
    IServiceProvider serviceProvider, 
    ILogger<CommandHandlerBase<TCommand, TResult>> logger) 
    : IRequestHandler<TCommand, Task<Result<TResult>>> 
    where TCommand : class, ITeamCommand
{
    /// <inheritdoc />
    public async Task<Result<TResult>> Handle(TCommand request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var teamModelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        logger.LogTrace("Loading model from event database...");
        ulong? expectedVersion = null;
        if (request is IConcurrentCommand concurrentCommand)
        {
            expectedVersion = concurrentCommand.ConcurrencyToken;
        }
        
        var model = await teamModelLoader.LoadModelAsync(request.Team, expectedVersion);
        if (model is null) return Result.Fail("Repository kann nicht gefunden werden.");
        logger.LogTrace("PreCommandExecution: Validate model and request...");
        var context = new CommandExecutionContext(model, request);
        await PrepareCommandAsync(context);
        if (!context.ResultObj.IsSuccess)
        {
            logger.LogInformation("Error during pre command execution: {ValidationErrors}.", string.Join(",", context.ResultObj.Errors.Select(e => e.Message)));
            return context.ResultObj;
        }
        
        logger.LogTrace("Applying command to model...");
        await ApplyCommandToModelAsync(context);
        if (!context.ResultObj.IsSuccess)
        {
            logger.LogInformation("Command failed: {ModelErrors}", string.Join(",", context.ResultObj.Errors.Select(e => e.Message)));
            return context.ResultObj;
        }
        
        logger.LogTrace("Retrieve model changes...");
        var changes = changeTracker.GetChanges();
        logger.LogInformation("Model changes found: [{ModelChangesCount}] Pcs.", changes.Count);
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        logger.LogTrace("Storing events to database...");
        CommitVersion = await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        logger.LogTrace("Stored events to database...");
        await PrepareResultAsync(context);
        return context.ResultObj;
    }

    /// <summary>
    /// Gets or sets the database version after storage.
    /// </summary>
    protected ulong? CommitVersion { get; set; }

    /// <summary>
    /// The command execution context.
    /// </summary>
    protected virtual Task PrepareResultAsync(ICommandExecutionContext context)
    {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Validates the model before executing the model command.
    /// </summary>
    protected virtual Task PrepareCommandAsync(ICommandExecutionContext context)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Applies the given command to the model.
    /// </summary>
    /// <param name="context">The command context object.</param>
    /// <returns></returns>
    protected abstract Task ApplyCommandToModelAsync(ICommandExecutionContext context);

    /// <summary>
    /// The command execution context.
    /// </summary>
    protected interface ICommandExecutionContext
    {
        /// <summary>
        /// Gets the Model.
        /// </summary>
        ITeamAggregate Model { get; }

        /// <summary>
        /// Gets the Request.
        /// </summary>
        TCommand Command { get; }

        /// <summary>
        /// Updates the command result.
        /// </summary>
        void SetResult(Result<TResult> result);
    }

    /// <summary>
    /// CommandExecutionContext that bundles relevant data for command execution.
    /// </summary>
    /// <param name="Model">The model.</param>
    /// <param name="Command">The command request.</param>
    protected record CommandExecutionContext(ITeamAggregate Model, TCommand Command) : ICommandExecutionContext
    {
        /// <summary>
        /// The command execution result.
        /// </summary>
        public Result<TResult> ResultObj { get; set; } = Result.Ok();

        /// <summary>
        /// Gets the Model.
        /// </summary>
        public ITeamAggregate Model { get; } = Model;

        /// <summary>
        /// Gets the Request.
        /// </summary>
        public TCommand Command { get; } = Command;

        /// <summary>
        /// Updates the command result.
        /// </summary>
        public void SetResult(Result<TResult> result)
        {
            if (result.IsFailed)
            {
                ResultObj.WithErrors(result.Errors);
            }
            else
            {
                ResultObj.WithValue(result.Value);
            }
        }
    }
}