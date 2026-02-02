using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

/// <summary>
/// The base query handler.
/// </summary>
/// <param name="serviceProvider">The service provider.</param>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public abstract class QueryHandlerBase<TQuery, TResult>(IServiceProvider serviceProvider) : IRequestHandler<TQuery, Task<TResult>>
    where TQuery : class, ITeamQuery
{
    /// <summary>
    /// The handler method. 
    /// </summary>
    public async Task<TResult> Handle(TQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.Team);
        return await GetResult(model, request);
    }

    /// <summary>
    /// Gets the result from the model.
    /// </summary>
    /// <param name="model">The team model.</param>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    protected abstract Task<TResult> GetResult(ITeamAggregate model, TQuery request);
}