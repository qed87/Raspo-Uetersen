using DispatchR.Abstractions.Send;
using FluentResults;
using StampCard.Backend.Commands.Shared.Interfaces;
using StampCard.Backend.Model;
using StampCard.Backend.Services;
using StampCard.Backend.Services.Interfaces;

namespace StampCard.Backend.Commands.Shared;

/// <summary>
/// The base query handler.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class QueryHandlerBase<TQuery, TResult>(
    IServiceProvider serviceProvider, 
    ILogger<QueryHandlerBase<TQuery, TResult>> logger) : IRequestHandler<TQuery, Task<Result<TResult>>>
    where TQuery : class, ITeamQuery
{
    /// <summary>
    /// The handler method. 
    /// </summary>
    public async Task<Result<TResult>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        logger.LogTrace("Loading team model...");
        var model = await modelLoader.LoadModelAsync(request.Team);
        if (model is null) return Result.Fail("Repository kann nicht gefunden werden.");
        logger.LogTrace("Query result from model...");
        return Result.Ok(await GetResult(model, request));
    }

    /// <summary>
    /// Gets the result from the model.
    /// </summary>
    /// <param name="model">The team model.</param>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected abstract Task<TResult> GetResult(ITeamAggregate model, TQuery request);
}