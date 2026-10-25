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

    /// <summary>
    /// Gets all stamps of the <param name="teamId">Team</param> that wait for approval.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    public Task<ResponseWrapperDto<List<PendingStampReadDto>>> ListPendingStampsAsync(string teamId)
    {
        var request = new RestRequest("/api/teams/{teamId}/stamp/pending")
            .AddUrlSegment("teamId", teamId);
        return restClient.SendAsync<List<PendingStampReadDto>>(request);
    }

    /// <summary>
    /// Approves the pending <param name="stampIds">Stamps</param> of the <param name="teamId">Team</param> at once.
    /// Either all or none of them are approved.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="stampIds">The ids of the pending stamps.</param>
    public Task<ResponseWrapperDto> ApproveStampsAsync(string teamId, IEnumerable<Guid> stampIds)
    {
        var request = new RestRequest("/api/teams/{teamId}/stamp/pending/approve", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddJsonBody(new { stampIds });
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Rejects the pending <param name="id">Stamp</param> of the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>,
    /// which removes it from the stamp card.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="id">The stamp id.</param>
    public Task<ResponseWrapperDto> RejectStampAsync(string teamId, Guid stampCardId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}/stamp/{stampId}/reject", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", stampCardId)
            .AddUrlSegment("stampId", id);
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Gets all share codes of the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="stampCardId">The stamp card id.</param>
    public Task<ResponseWrapperDto<List<SharedCodeReadDto>>> ListSharedCodesAsync(string teamId, Guid stampCardId)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}/share")
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", stampCardId);
        return restClient.SendAsync<List<SharedCodeReadDto>>(request);
    }

    /// <summary>
    /// Creates a new share code for the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="holderName">Name of the player or parent the code is issued to, stamps submitted with it are issued in this name.</param>
    public Task<ResponseWrapperDto<SharedCodeReadDto>> ShareAsync(string teamId, Guid stampCardId, string holderName)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}/share", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", stampCardId)
            .AddJsonBody(new { holderName });
        return restClient.SendAsync<SharedCodeReadDto>(request);
    }

    /// <summary>
    /// Revokes the <param name="id">share code</param> of the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// The stamp card can still be read with it, but no more stamps can be submitted.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="id">The share code id.</param>
    public Task<ResponseWrapperDto> RevokeSharedCodeAsync(string teamId, Guid stampCardId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}/share/{sharedCodeId}/revoke", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", stampCardId)
            .AddUrlSegment("sharedCodeId", id);
        return restClient.SendAsync(request);
    }

    /// <summary>
    /// Deletes the <param name="id">share code</param> from the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="id">The share code id.</param>
    public Task<ResponseWrapperDto> DeleteSharedCodeAsync(string teamId, Guid stampCardId, Guid id)
    {
        var request = new RestRequest("/api/teams/{teamId}/stampcard/{stampCardId}/share/{sharedCodeId}", Method.Delete)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", stampCardId)
            .AddUrlSegment("sharedCodeId", id);
        return restClient.SendAsync(request);
    }
}
