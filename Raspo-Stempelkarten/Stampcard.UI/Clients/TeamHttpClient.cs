using System.Text.Json;
using RestSharp;
using Stampcard.UI.Dtos;

namespace Stampcard.UI.Clients;

public class TeamHttpClient(IRestClient restClient)
{
    public async Task<ResponseWrapperDto<TeamDetailedReadDto>> GetTeamAsync(string id)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<TeamDetailedReadDto>>($"/api/teams/{id}");
        return response.Data!;
    }
    
    public async Task<ResponseWrapperDto<List<string>>> ListCoachesAsync(string id)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<string>>>($"/api/teams/{id}/coach");
        return response.Data!;
    }
    

    public async Task<ResponseWrapperDto<List<TeamReadDto>>> ListTeamsAsync()
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<TeamReadDto>>>("/api/teams");
        return response.Data!;
    }

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

    public async Task<ResponseWrapperDto> DeleteTeamAsync(string id)
    {
        var request = new RestRequest($"/api/teams/{id}");
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    public async Task<ResponseWrapperDto> CreateCoachAsync(string id, string name)
    {
        var request = new RestRequest($"/api/teams/{id}/coach/{name}");
        var response = await restClient.ExecutePostAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    public async Task<ResponseWrapperDto> DeleteCoachAsync(string id, string name)
    {
        var request = new RestRequest($"/api/teams/{id}/coach/{name}");
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    public async Task<ResponseWrapperDto> UpdateTeamAsync(string name, ulong concurrencyToken)
    {
        var request = new RestRequest("/api/teams");
        request.AddParameter("name", name, ParameterType.GetOrPost);
        request.AddParameter("concurrencyToken", concurrencyToken, ParameterType.GetOrPost);
        var response = await restClient.ExecutePutAsync(request);
        return JsonSerializer.Deserialize<ResponseWrapperDto>(
            response.Content!, 
            new JsonSerializerOptions {  PropertyNameCaseInsensitive = true })!;
    }
}