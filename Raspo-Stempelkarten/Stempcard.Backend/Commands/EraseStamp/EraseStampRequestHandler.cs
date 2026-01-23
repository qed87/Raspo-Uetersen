using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.EraseStamp;

public class EraseStampRequestHandler(IServiceProvider serviceProvider) : IRequestHandler<EraseStamp, Task<Result<EraseStampResponse>>>
{
    public async Task<Result<EraseStampResponse>> Handle(EraseStamp request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = model.EraseStamp(request.StampCardId, request.StampId);
        if (!result.IsSuccess) return result.ToResult();
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IStampModelStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new EraseStampResponse { Id = result.Value.Id });
    }
}