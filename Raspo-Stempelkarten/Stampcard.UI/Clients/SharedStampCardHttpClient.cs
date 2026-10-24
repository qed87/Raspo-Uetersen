using System.Net;
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

    /// <summary>
    /// Submits a stamp to the Stamp card with the <param name="id">Stamp card id</param> from the given <param name="teamId">Team</param>.
    /// The stamp is pending until a coach approves it.
    /// </summary>
    /// <param name="teamId">The team id.</param>
    /// <param name="id">The stamp card id.</param>
    /// <param name="code">The share code.</param>
    /// <param name="reason">Reason for the stamp.</param>
    /// <param name="issuerName">Name of the external user submitting the stamp.</param>
    public async Task<ResponseWrapperDto> SubmitStampAsync(string teamId, Guid id, string code, string reason, string issuerName)
    {
        var request = new RestRequest("/api/shared/teams/{teamId}/stampcard/{stampCardId}/stamp", Method.Post)
            .AddUrlSegment("teamId", teamId)
            .AddUrlSegment("stampCardId", id)
            .AddHeader(ShareCodeHeader, code)
            .AddJsonBody(new { reason, issuerName });
        var response = await restClient.ExecuteAsync(request);
        // external users get german messages instead of the english ones of the backend
        return response.StatusCode switch
        {
            HttpStatusCode.Created => new ResponseWrapperDto(),
            HttpStatusCode.Forbidden => ResponseWrapperDto.Fail(
                "Dieser Link wurde widerrufen. Es können keine Stempel mehr eingereicht werden."),
            HttpStatusCode.NotFound => ResponseWrapperDto.Fail(
                "Die Stempelkarte wurde nicht gefunden oder der Link ist nicht mehr gültig."),
            HttpStatusCode.TooManyRequests => ResponseWrapperDto.Fail(
                "Zu viele Anfragen. Bitte versuchen Sie es in einer Minute erneut."),
            HttpStatusCode.BadRequest => ResponseWrapperDto.Fail(
                "Bitte geben Sie einen Grund und Ihren Namen (mindestens 2 Zeichen) an."),
            _ => ResponseWrapperDto.Fail("Der Stempel konnte nicht eingereicht werden. Bitte versuchen Sie es später erneut.")
        };
    }
}
