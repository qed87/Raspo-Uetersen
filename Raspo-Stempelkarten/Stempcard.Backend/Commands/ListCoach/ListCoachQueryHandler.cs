using DispatchR.Abstractions.Send;
using Raspo_Stempelkarten_Backend.Core;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Commands.ListCoach;

/// <inheritdoc />
public class ListCoachQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<ListCoachQuery, Task<List<string>>>
{
    /// <inheritdoc />
    public async Task<List<string>> Handle(ListCoachQuery request, CancellationToken cancellationToken)
    {
        using var disposable = serviceProvider.CreateScope();
        var modelLoader = serviceProvider.GetRequiredService<ITeamModelLoader>();
        var model = await modelLoader.LoadModelAsync(request.TeamId);
        return model.Coaches.Select(coach => coach.Email).ToList();
    }
}