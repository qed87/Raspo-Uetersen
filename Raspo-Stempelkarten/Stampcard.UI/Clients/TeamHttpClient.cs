using System.Text.Json;
using RestSharp;
using Stampcard.Contracts.Dtos;

namespace Stampcard.UI.Clients;

/// <summary>
/// Rest client for accessing teams.
/// </summary>
/// <param name="restClient">The underlying rest client.</param>
public class TeamHttpClient(IRestClient restClient)
{
    /// <summary>
    /// Gets the team with the given <param name="id">Id</param>.
    /// </summary>
    /// <param name="id">The team id (stream name).</param>
    public async Task<ResponseWrapperDto<TeamDetailedReadDto>> GetTeamAsync(string id)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<TeamDetailedReadDto>>($"/api/teams/{id}");
        return response.Data!;
    }
    
    /// <summary>
    /// Gets all coaches of the team with the given <param name="id">Id</param>.
    /// </summary>
    /// <param name="id">The team id (stream name).</param>
    public async Task<ResponseWrapperDto<List<string>>> ListCoachesAsync(string id)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<string>>>($"/api/teams/{id}/coach");
        return response.Data!;
    }
    
    /// <summary>
    /// Gets all teams.
    /// </summary>
    public async Task<ResponseWrapperDto<List<TeamReadDto>>> ListTeamsAsync()
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<TeamReadDto>>>("/api/teams");
        return response.Data!;
    }

    /// <summary>
    /// Creates a new team with the <param name="name">Name</param>.
    /// </summary>
    /// <param name="name">The team name.</param>
    public async Task<ResponseWrapperDto> CreateTeamAsync(string name)
    {
        var request = new RestRequest("/api/teams");
        request.AddParameter("club", "raspo1926", ParameterType.GetOrPost);
        request.AddParameter("name", name, ParameterType.GetOrPost);
        var response = await restClient.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<ResponseWrapperDto>(
            response.Content!, 
            new JsonSerializerOptions {  PropertyNameCaseInsensitive = true })!;
    }

    /// <summary>
    /// Deletes the team with the <param name="id">Id</param>.
    /// </summary>
    /// <param name="id">The team id (stream name).</param>
    public async Task<ResponseWrapperDto> DeleteTeamAsync(string id)
    {
        var request = new RestRequest($"/api/teams/{id}");
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    /// <summary>
    /// Creates a new coach for a team.
    /// </summary>
    /// <param name="id">The team id (stream name).</param>
    /// <param name="name">The name (e-mail) of the coach.</param>
    public async Task<ResponseWrapperDto> CreateCoachAsync(string id, string name)
    {
        var request = new RestRequest($"/api/teams/{id}/coach/");
        request.AddOrUpdateParameter("name", name, ParameterType.GetOrPost);
        var response = await restClient.ExecutePostAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    /// <summary>
    /// Deletes the coach with <param name="name">Name</param> from the team with <param name="id">Id</param>.
    /// </summary>
    /// <param name="id">The team id (stream name).</param>
    /// <param name="name">The name (e-mail) of the coach.</param>
    public async Task<ResponseWrapperDto> DeleteCoachAsync(string id, string name)
    {
        var request = new RestRequest($"/api/teams/{id}/coach/{name}");
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    /// <summary>
    /// Updates the team with <param name="id">Id</param> and updates the <param name="name">Name</param>.
    /// </summary>
    /// <param name="id">The team id (stream name).</param>
    /// <param name="name">New team name.</param>
    /// <param name="concurrencyToken">The concurrency token.</param>
    public async Task<ResponseWrapperDto> UpdateTeamAsync(string id, string name, ulong concurrencyToken)
    {
        var request = new RestRequest($"/api/teams/{id}");
        request.AddParameter("name", name, ParameterType.GetOrPost);
        request.AddParameter("concurrencyToken", concurrencyToken, ParameterType.GetOrPost);
        var response = await restClient.ExecutePutAsync(request);
        return JsonSerializer.Deserialize<ResponseWrapperDto>(
            response.Content!, 
            new JsonSerializerOptions {  PropertyNameCaseInsensitive = true })!;
    }
}