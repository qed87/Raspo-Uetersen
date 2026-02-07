using System.Text.Json;
using KurrentDB.Client;
using StampCard.Backend.Queries.ListTeamsQuery;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Services;

/// <summary>
/// All teams.
/// </summary>
public class TeamService(KurrentDBProjectionManagementClient kurrentDbProjectionManagementClient, ILogger<TeamService> logger) : ITeamService
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
        logger.LogDebug("Loaded projection 'all-club-teams' with content '{Content}'.", JsonSerializer.Serialize(state));
        return state.Teams ?? [];
    }
}