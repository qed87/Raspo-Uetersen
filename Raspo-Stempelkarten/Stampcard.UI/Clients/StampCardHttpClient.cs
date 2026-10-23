using RestSharp;

namespace Stampcard.UI.Clients;

/// <summary>
/// Rest client for accessing stamp cards.
/// </summary>
/// <param name="restClient">The underlying rest client.</param>
public class StampCardHttpClient(IRestClient restClient)
{
    /// <summary>
    /// Gets the Stamp card including its stamps with the <param name="id">Stamp card id</param> from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="id">The stamp card id.</param>
    public Task<ResponseWrapperDto<StampCardReadDto>> GetAsync(string teamId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}")
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", id);
        return restClient.SendAsync<StampCardReadDto>(request);
    }

    /// <summary>
    /// Gets all Stamp cards from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="accountingYear">Only stamp cards of this accounting year, all if not set.</param>
    public Task<ResponseWrapperDto<List<StampCardReadDto>>> ListAsync(string teamId, int? accountingYear = null)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard")
            .AddUrlSegment("teamId", teamId);
        if (accountingYear is not null) request.AddQueryParameter("accountingYear", accountingYear.Value);
        return restClient.SendAsync<List<StampCardReadDto>>(request);
    }

    /// <summary>
    /// Creates a new stamp card for the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="playerId">The player id.</param>
    /// <param name="accountingYear">The accounting year.</param>
    public Task<ResponseWrapperDto> CreateAsync(string teamId, Guid playerId, int accountingYear)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddJsonBody(new { playerId, accountingYear });
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Delete the Stamp card with the <param name="id">Stamp card id</param> from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="id">The stamp card id.</param>
    public Task<ResponseWrapperDto> DeleteAsync(string teamId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}", Method.Delete)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", id);
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Stamps the <param name="id">stamp card</param> from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="id">The stamp card id.</param>
    /// <param name="reason">Reason for the stamp.</param>
    public Task<ResponseWrapperDto> StampAsync(string teamId, Guid id, string reason)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}/stamp", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", id)
            .AddJsonBody(new { reason });
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Deletes the <param name="id">Stamp</param> from the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="id">The stamp id.</param>
    public Task<ResponseWrapperDto> DeleteStampAsync(string teamId, Guid stampCardId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}/stamp/{stampId}", Method.Delete)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", stampCardId)
            .AddUrlSegment("stampId", id);
        return restClient.SendAsync(request);
    }
}
