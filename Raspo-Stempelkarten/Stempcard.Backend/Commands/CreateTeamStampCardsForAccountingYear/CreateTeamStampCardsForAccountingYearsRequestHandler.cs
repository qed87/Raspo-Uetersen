using DispatchR.Abstractions.Send;
using FluentResults;
using Raspo_Stempelkarten_Backend.Core;

namespace Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;

/// <inheritdoc />
public class CreateTeamStampCardsForAccountingYearsRequestHandler(IServiceProvider serviceProvider)
    : IRequestHandler<CreateTeamStampCardsForAccountingYears, Task<Result<CreateTeamStampCardsForAccountingYearsResponse>>>
{
    /// <inheritdoc />
    public async Task<Result<CreateTeamStampCardsForAccountingYearsResponse>> Handle(
        CreateTeamStampCardsForAccountingYears request, 
        CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var changeTracker = serviceProvider.GetRequiredService<IEventDataChangeTracker>();
        var stampModelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await stampModelLoader.LoadModelAsync(request.Team);
        var result = await model.CreateNewAccountingYearAsync(request.AccountingYear);
        if (!result.IsSuccess) return result;
        var changes = changeTracker.GetChanges();
        var storage = serviceProvider.GetRequiredService<IEventStorage>();
        await storage.StoreAsync(request.Team, model.Version, changes, cancellationToken);
        return Result.Ok(new CreateTeamStampCardsForAccountingYearsResponse());
    }
}