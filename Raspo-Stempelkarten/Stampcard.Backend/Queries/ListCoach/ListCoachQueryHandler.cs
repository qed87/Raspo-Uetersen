using StampCard.Backend.Commands.Shared;
using StampCard.Backend.Model;

namespace StampCard.Backend.Queries.ListCoach;

/// <inheritdoc />
public class ListCoachQueryHandler(IServiceProvider serviceProvider, ILogger<ListCoachQueryHandler> logger) 
    : QueryHandlerBase<ListCoachQuery, List<string>>(serviceProvider, logger)
{
    /// <inheritdoc />
    protected override Task<List<string>> GetResult(ITeamAggregate model, ListCoachQuery request)
    {
        return Task.FromResult(model.Coaches.Select(coach => coach.Email).ToList());
    }
}