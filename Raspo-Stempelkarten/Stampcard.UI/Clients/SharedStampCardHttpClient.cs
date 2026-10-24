using RestSharp;

namespace Stampcard.UI.Clients;

/// <summary>
/// Rest client for external users accessing a shared stamp card. Requests are authorized by the share code
/// instead of an access token.
/// </summary>
/// <param name="restClient">The underlying rest client without authenticator.</param>
public class SharedStampCardHttpClient(IRestClient restClient)
{
    private const string ShareCodeHeader = "X-Share-Code";

    /// <summary>
    /// Gets the Stamp card with the <param name="id">Stamp card id</param> from the given <param name="teamId">Team</param>.
    /// Issuers of the stamps are not returned.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="id">The stamp card id.</param>
    /// <param name="code">The share code.</param>
    public Task<ResponseWrapperDto<StampCardReadDto>> GetAsync(string teamId, Guid id, string code)
    {
        var request = new RestRequest("/api/shared/teams/{teamId}/stampcard/{stampCardId}")
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", id)
            .AddHeader(ShareCodeHeader, code);
        return restClient.SendAsync<StampCardReadDto>(request);
    }
}
