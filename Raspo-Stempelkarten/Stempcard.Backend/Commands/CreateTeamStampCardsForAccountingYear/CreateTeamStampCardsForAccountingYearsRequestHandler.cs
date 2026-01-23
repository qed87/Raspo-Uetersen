using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Commands.Shared;

namespace Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;

public class CreateTeamStampCardsForAccountingYearsRequestHandler(IServiceProvider serviceProvider)
    : IRequestHandler<CreateTeamStampCardsForAccountingYears, Task<Result<CreateTeamStampCardsForAccountingYearsResponse>>>
{
    public async Task<Result<CreateTeamStampCardsForAccountingYearsResponse>> Handle(
        CreateTeamStampCardsForAccountingYears request, 
        CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<IStampModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = model.CreateNewAccountingYear(request.AccountingYear);
        if (!result.IsSuccess) return result.ToResult();
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IStampModelStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new CreateTeamStampCardsForAccountingYearsResponse { Ids = result.Value.Select(card => card.Id).ToArray() });
    }
}