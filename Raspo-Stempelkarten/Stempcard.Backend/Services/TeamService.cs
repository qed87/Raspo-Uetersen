using System.Text.Json;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

namespace Raspo_Stempelkarten_Backend.Services;

/// <summary>
/// All teams.
/// </summary>
public class TeamService(KurrentDBProjectionManagementClient kurrentDbProjectionManagementClient) : ITeamService
{
    private const string StreamName = "all-club-teams";
    
    /// <summary>
    /// List all available teams.
    /// </summary>
    public async Task<List<TeamReadDto>> ListTeamsAsync(CancellationToken cancellationToken = default)
    {
        var state = await kurrentDbProjectionManagementClient.GetStateAsync<AllClubTeamsState>(
            StreamName, cancellationToken: cancellationToken, serializerOptions: new JsonSerializerOptions
            { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true });
        if (state.Teams is null) return [];
        return state.Teams;
    }
}