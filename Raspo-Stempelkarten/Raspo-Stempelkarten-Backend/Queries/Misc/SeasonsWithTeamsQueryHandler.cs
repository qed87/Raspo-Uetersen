using DispatchR.Abstractions.Send;
using JetBrains.Annotations;
using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Queries.Misc;

[UsedImplicitly]
public class SeasonsWithTeamsQueryHandler(KurrentDBProjectionManagementClient projectionManagementClient)
    : IRequestHandler<GetSeasonsWithTeamsQuery, Task<SeasonsResult>>
{
    public async Task<SeasonsResult> Handle(GetSeasonsWithTeamsQuery request, CancellationToken cancellationToken)
    {
        var result = await projectionManagementClient.GetResultAsync<SeasonsResult>(
            "StampCard-Seasons-and-Teams",
            cancellationToken: cancellationToken);
        return result;
    }
}