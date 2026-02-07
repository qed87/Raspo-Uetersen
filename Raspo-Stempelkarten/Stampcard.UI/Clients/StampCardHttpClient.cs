using RestSharp;
using Stampcard.Contracts.Dtos;

namespace Stampcard.UI.Clients;

/// <summary>
/// Rest client for accessing stamp cards.
/// </summary>
/// <param name="restClient">The underlying rest client.</param>
public class StampCardHttpClient(IRestClient restClient)
{
    /// <summary>
    /// Gets the Stamp card with the <param name="id">Stamp card id</param> from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="id">The stamp card id.</param>
    public async Task<ResponseWrapperDto<StampCardReadDetailsDto>> GetAsync(string teamId, Guid id)
    {
        var request = new RestRequest($"/api/teams/{teamId}/stampcards/{id}");
        request.AddOrUpdateParameter("includeDetails", true, ParameterType.GetOrPost);
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<StampCardReadDetailsDto>>($"/api/teams/{teamId}/stampcards/");
        return response.Data!;
    }
    
    /// <summary>
    /// Gets all Stamp cards from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    public async Task<ResponseWrapperDto<List<StampCardReadDto>>> ListAsync(string teamId)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<StampCardReadDto>>>($"/api/teams/{teamId}/stampcards/");
        return response.Data!;
    }
    
    /// <summary>
    /// Creates a new stamp card for the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="playerId">The player id.</param>
    /// <param name="accountingYear">The accounting year.</param>
    public async Task<ResponseWrapperDto> CreateAsync(string teamId, Guid playerId, int accountingYear)
    {
        var request = new RestRequest($"api/teams/{teamId}/players", Method.Post);
        request.AddOrUpdateParameter("playerId", playerId, ParameterType.GetOrPost);
        request.AddOrUpdateParameter("accountingYear", accountingYear, ParameterType.GetOrPost);
        var response = await restClient.ExecutePostAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }
    
    /// <summary>
    /// Delete the Stamp card with the <param name="id">Stamp card id</param> from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="id">The stamp card id.</param>
    public async Task<ResponseWrapperDto> DeleteAsync(string teamId, string id)
    {
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>($"api/teams/{teamId}/stampcards/{id}");
        return response.Data!;
    }
    
    /// <summary>
    /// Stamps the <param name="id">stamp card</param> from the given <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="id">The stamp card id.</param>
    /// <param name="reason">Reason for the stamp.</param>
    public async Task<ResponseWrapperDto> StampAsync(string teamId, Guid id, string reason)
    {
        var request = new RestRequest($"api/teams/{teamId}/stampcard/{id}/stamps", Method.Post);
        request.AddOrUpdateParameter("reason", reason, ParameterType.GetOrPost);
        var response = await restClient.ExecutePostAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }
    
    /// <summary>
    /// Gets the <param name="id">Stamp</param> from the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="id">The stamp id.</param>
    public async Task<ResponseWrapperDto<StampReadDto>> GetStampAsync(string teamId, Guid stampCardId, Guid id)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<StampReadDto>>($"/api/teams/{teamId}/stampcards/{stampCardId}/stamps/{id}");
        return response.Data!;
    }
    
    /// <summary>
    /// List all stamps from the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="stampCardId">The stamp card id.</param>
    public async Task<ResponseWrapperDto<List<StampReadDto>>> ListStampsAsync(string teamId, string stampCardId)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<StampReadDto>>>($"/api/teams/{teamId}/stampcards/{stampCardId}/stamps/");
        return response.Data!;
    }
    
    /// <summary>
    /// Deletes the <param name="id">Stamp</param> from the <param name="stampCardId">StampCard</param> in <param name="teamId">Team</param>.
    /// </summary>
    /// <param name="teamId">The team id (stream name).</param>
    /// <param name="stampCardId">The stamp card id.</param>
    /// <param name="id">The stamp id.</param>
    public async Task<ResponseWrapperDto> DeleteStampAsync(string teamId, Guid stampCardId, Guid id)
    {
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>($"api/teams/{teamId}/stampcards/{stampCardId}/stamps/{id}");
        return response.Data!;
    }
}