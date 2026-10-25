using RestSharp;

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
    /// <param name="id">The team id.</param>
    public Task<ResponseWrapperDto<TeamReadDto>> GetTeamAsync(string id)
    {
        var request = new RestRequest("/api/teams/{teamId}")
            .AddUrlSegment("teamId", id);
        return restClient.SendAsync<TeamReadDto>(request);
    }

    /// <summary>
    /// Gets all teams visible to the current user.
    /// </summary>
    public Task<ResponseWrapperDto<List<TeamReadDto>>> ListTeamsAsync()
    {
        return restClient.SendAsync<List<TeamReadDto>>(new RestRequest("/api/teams"));
    }

    /// <summary>
    /// Creates a new team with the <param name="name">Name</param>.
    /// </summary>
    /// <param name="name">The team name.</param>
    public Task<ResponseWrapperDto> CreateTeamAsync(string name)
    {
        var request = new RestRequest("/api/teams", Method.Post)
            .AddJsonBody(new { name });
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Deletes the team with the <param name="id">Id</param>.
    /// </summary>
    /// <param name="id">The team id.</param>
    public Task<ResponseWrapperDto> DeleteTeamAsync(string id)
    {
        var request = new RestRequest("/api/teams/{teamId}", Method.Delete)
            .AddUrlSegment("teamId", id);
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Adds a coach to a team.
    /// </summary>
    /// <param name="id">The team id.</param>
    /// <param name="email">The e-mail of the coach.</param>
    public Task<ResponseWrapperDto> CreateCoachAsync(string id, string email)
    {
        var request = new RestRequest("/api/teams/{teamId}/coach", Method.Post)
            .AddUrlSegment("teamId", id)
            .AddJsonBody(new { email });
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Removes the coach with <param name="email">E-Mail</param> from the team with <param name="id">Id</param>.
    /// </summary>
    /// <param name="id">The team id.</param>
    /// <param name="email">The e-mail of the coach.</param>
    public Task<ResponseWrapperDto> DeleteCoachAsync(string id, string email)
    {
        var request = new RestRequest("/api/teams/{teamId}/coach/{email}", Method.Delete)
            .AddUrlSegment("teamId", id)
            .AddUrlSegment("email", email);
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Renames the team with <param name="id">Id</param>.
    /// </summary>
    /// <param name="id">The team id.</param>
    /// <param name="name">New team name.</param>
    /// <param name="concurrencyToken">The concurrency token of the team last read.</param>
    public Task<ResponseWrapperDto> UpdateTeamAsync(string id, string name, long concurrencyToken)
    {
        var request = new RestRequest("/api/teams/{teamId}", Method.Put)
            .AddUrlSegment("teamId", id)
            .AddJsonBody(new { name, concurrencyToken });
        return restClient.SendAsync(request);
    }
}
