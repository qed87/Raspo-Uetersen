using RestSharp;
using Stampcard.Contracts.Dtos;

namespace Stampcard.UI.Clients;

/// <summary>
/// The players http client.
/// </summary>
/// <param name="restClient">The underlying rest client.</param>
public class PlayerHttpClient(IRestClient restClient)
{
    /// <summary>
    /// Gets the player with <param name="id">Player id</param> from the given <param name="teamId">Team Id</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="id">The player id.</param>
    public async Task<ResponseWrapperDto<PlayerReadDto>> GetAsync(string teamId, Guid id)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<PlayerReadDto>>($"/api/teams/{teamId}/players/{id}");
        return response.Data!;
    }
    
    /// <summary>
    /// Gets the players from the given <param name="teamId">Team Id</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    public async Task<ResponseWrapperDto<List<PlayerReadDto>>> ListAsync(string teamId)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<PlayerReadDto>>>($"/api/teams/{teamId}/players/");
        return response.Data ?? ResponseWrapperDto<List<PlayerReadDto>>.Fail("Unerwarteter Fehler.");
    }
    
    /// <summary>
    /// Add a new player to the <param name="teamId">Team Id</param>. 
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="firstName">The first name of the player to add.</param>
    /// <param name="lastName">The last name of the player to add.</param>
    /// <param name="birthDate">The birthdate of the player to add.</param>
    /// <param name="birthplace">The birthplace of the player to add.</param>
    public async Task<ResponseWrapperDto> CreateAsync(string teamId, string firstName, 
        string lastName, DateOnly birthDate, string birthplace)
    {
        var request = new RestRequest($"api/teams/{teamId}/players", Method.Post);
        request.AddOrUpdateParameter("firstName", firstName, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("lastName", lastName, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("birthDate", birthDate, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("birthPlace", birthplace, ParameterType.GetOrPost);
        var response = await restClient.ExecutePostAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }
    
    /// <summary>
    /// Updates the <param name="playerId">Player</param> of the <param name="team">Team</param>.
    /// </summary>
    /// <param name="team">The team id (stream name).</param>
    /// <param name="playerId">The player id.</param>
    /// <param name="firstName">The first name of the player to add.</param>
    /// <param name="lastName">The last name of the player to add.</param>
    /// <param name="birthdate">The birthdate of the player to add.</param>
    /// <param name="birthplace">The birthplace of the player to add.</param>
    /// <param name="active">A value indicating whether the player is active or inactive.</param>
    /// <param name="concurrencyToken">The concurrency token.</param>
    public async Task<ResponseWrapperDto> UpdateAsync(string team, Guid playerId, string firstName, string lastName, 
        DateOnly birthdate, string birthplace, bool active, ulong concurrencyToken)
    {
        var request = new RestRequest($"api/teams/{team}/players/{playerId}", Method.Post);
        request.AddOrUpdateParameter("firstName", firstName, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("lastName", lastName, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("birthDate", birthdate, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("birthPlace", birthplace, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("active", active, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("concurrencyToken", concurrencyToken, ParameterType.GetOrPost);
        var response = await restClient.ExecutePutAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }
    
    /// <summary>
    /// Deletes the player with <param name="id">Player id</param> from the given <param name="teamId">Team Id</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="id">The player id.</param>
    public async Task<ResponseWrapperDto> DeleteAsync(string teamId, Guid id)
    {
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>($"api/teams/{teamId}/players/{id}");
        return response.Data!;
    }
}