using RestSharp;

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
    /// <param name="teamId">The team id.</param>
    /// <param name="id">The player id.</param>
    public Task<ResponseWrapperDto<PlayerReadDto>> GetAsync(string teamId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/player/{playerId}")
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("playerId", id);
        return restClient.SendAsync<PlayerReadDto>(request);
    }

    /// <summary>
    /// Gets the players from the given <param name="teamId">Team Id</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    public Task<ResponseWrapperDto<List<PlayerReadDto>>> ListAsync(string teamId)
    {
        var request = new RestRequest("/api/teams/{teamId}/player")
            .AddUrlSegment("teamId", teamId);
        return restClient.SendAsync<List<PlayerReadDto>>(request);
    }

    /// <summary>
    /// Add a new player to the <param name="teamId">Team Id</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="firstName">The first name of the player to add.</param>
    /// <param name="lastName">The last name of the player to add.</param>
    /// <param name="birthdate">The birthdate of the player to add.</param>
    public Task<ResponseWrapperDto> CreateAsync(string teamId, string firstName, string lastName, DateOnly birthdate)
    {
        var request = new RestRequest("/api/teams/{teamId}/player", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddJsonBody(new { firstName, lastName, birthdate });
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Updates the <param name="playerId">Player</param> of the <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="playerId">The player id.</param>
    /// <param name="firstName">The first name of the player.</param>
    /// <param name="lastName">The last name of the player.</param>
    /// <param name="birthdate">The birthdate of the player.</param>
    /// <param name="concurrencyToken">The concurrency token of the team last read.</param>
    public Task<ResponseWrapperDto> UpdateAsync(string teamId, Guid playerId, string firstName, string lastName,
        DateOnly birthdate, long concurrencyToken)
    {
        var request = new RestRequest("/api/teams/{teamId}/player/{playerId}", Method.Put)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("playerId", playerId)
            .AddJsonBody(new { firstName, lastName, birthdate, concurrencyToken });
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Enables or disables the <param name="playerId">Player</param> of the <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="playerId">The player id.</param>
    /// <param name="active">A value indicating whether the player is active or inactive.</param>
    public Task<ResponseWrapperDto> SetActiveAsync(string teamId, Guid playerId, bool active)
    {
        var request = new RestRequest($"/api/teams/{{teamId}}/player/{{playerId}}/{(active ? "enable" : "disable")}", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("playerId", playerId);
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Deletes the player with <param name="id">Player id</param> from the given <param name="teamId">Team Id</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="id">The player id.</param>
    public Task<ResponseWrapperDto> DeleteAsync(string teamId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/player/{playerId}", Method.Delete)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("playerId", id);
        return restClient.SendAsync(request);
    }
}
