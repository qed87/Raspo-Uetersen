using System.Text.Json;
using DispatchR.Abstractions.Send;
using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

public class ListTeamsQueryHandler(
    IServiceProvider serviceProvider, 
    KurrentDBProjectionManagementClient kurrentDbProjectionManagementClient) 
    : IRequestHandler<ListTeamsQuery, Task<List<string>>>
{
    public async Task<List<string>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var state = await kurrentDbProjectionManagementClient.GetStateAsync<TeamOutputState>(
            "Teams-Stream", cancellationToken: cancellationToken, serializerOptions: new JsonSerializerOptions  
                { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true });
        return state.Streams;
    }
}