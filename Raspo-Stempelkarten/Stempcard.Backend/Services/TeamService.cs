using System.Text.Json;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Commands.ListTeamsQuery;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Services;

/// <summary>
/// All teams.
/// </summary>
public class TeamService(KurrentDBProjectionManagementClient kurrentDbProjectionManagementClient) : ITeamService
{
    /// <summary>
    /// List all available teams.
    /// </summary>
    public async Task<List<TeamReadDto>?> ListTeamsAsync(CancellationToken cancellationToken = default)
    {
        var state = await kurrentDbProjectionManagementClient.GetStateAsync<AllClubTeamsState>(
            "all-club-teams", cancellationToken: cancellationToken, serializerOptions: new JsonSerializerOptions
                { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true });
        return state.Teams;
    }
}