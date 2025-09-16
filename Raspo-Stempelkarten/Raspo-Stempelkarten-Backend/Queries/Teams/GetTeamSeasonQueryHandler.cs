using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Queries.Teams;

[UsedImplicitly]
public class GetTeamSeasonQueryHandler(KurrentDBProjectionManagementClient projectionManagementClient)
    : IRequestHandler<GetTeamSeasonQuery, Task<StampCardTeamsAndSeasonsResult>>
{
    public async Task<StampCardTeamsAndSeasonsResult> Handle(GetTeamSeasonQuery request, CancellationToken cancellationToken)
    {
        var result = await projectionManagementClient.GetResultAsync<StampCardTeamsAndSeasonsResult>(
            "StampCard-Teams-and-Seasons",
            cancellationToken: cancellationToken);
        return result;
    }
}