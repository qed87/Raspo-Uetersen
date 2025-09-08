using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Queries.Teams;

[UsedImplicitly]
public class TeamQueryHandler(KurrentDBProjectionManagementClient projectionManagementClient)
    : IRequestHandler<ListTeamsQuery, Task<IEnumerable<string>>>
{
    public async Task<IEnumerable<string>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var result = await projectionManagementClient.GetResultAsync<StampCardTeamsAndSeasonsResult>(
            "StampCard-Teams-and-Seasons",
            cancellationToken: cancellationToken);
        return result.Teams;
    }
}