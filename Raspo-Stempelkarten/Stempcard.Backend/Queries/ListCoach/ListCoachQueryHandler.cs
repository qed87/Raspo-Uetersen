using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Commands.Shared;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Services;

namespace Raspo_Stempelkarten_Backend.Queries.ListCoach;

/// <inheritdoc />
public class ListCoachQueryHandler(IServiceProvider serviceProvider) 
    : QueryHandlerBase<ListCoachQuery, List<string>>(serviceProvider)
{
    protected override Task<List<string>> GetResult(ITeamAggregate model, ListCoachQuery request)
    {
        return Task.FromResult(model.Coaches.Select(coach => coach.Email).ToList());
    }
}