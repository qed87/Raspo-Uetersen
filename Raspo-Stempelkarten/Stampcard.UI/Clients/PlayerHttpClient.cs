using System.Text.Json;
using RestSharp;
using Stampcard.UI.Dtos;
using ResponseWrapperDto = Stampcard.UI.Dtos.ResponseWrapperDto;
using TeamReadDto = Stampcard.UI.Dtos.TeamReadDto;

namespace Stampcard.UI.Clients;

public class PlayerHttpClient(IRestClient restClient)
{
    public async Task<ResponseWrapperDto<PlayerReadDto>> GetAsync(string teamId, string id)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<PlayerReadDto>>($"/api/teams/{teamId}/players/{id}");
        return response.Data!;
    }
    
    public async Task<ResponseWrapperDto<List<PlayerReadDto>>> ListAsync(string teamId)
    {
        var response = await restClient.ExecuteGetAsync<ResponseWrapperDto<List<PlayerReadDto>>>($"/api/teams/{teamId}/players");
        return response.Data!;
    }
    
    public async Task<ResponseWrapperDto> CreateAsync(string teamId, string firstName, string lastName, DateOnly birthdate, string birthplace)
    {
        var request = new RestRequest($"/api/teams{teamId}/players");
        request.AddJsonBody(new PlayerCreateDto(firstName, lastName, birthdate, birthplace));
        var response = await restClient.ExecutePostAsync(request);
        return JsonSerializer.Deserialize<ResponseWrapperDto>(
            response.Content!, 
            new JsonSerializerOptions {  PropertyNameCaseInsensitive = true })!;
    }

    public async Task<ResponseWrapperDto> DeleteTeamAsync(string teamId, string id)
    {
        var request = new RestRequest($"/api/teams/{teamId}/players/{id}");
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    public async Task<ResponseWrapperDto> CreateCoachAsync(string id, string name)
    {
        var request = new RestRequest($"/api/teams/{id}/coach/");
        request.AddOrUpdateParameter("name", name, ParameterType.GetOrPost);
        var response = await restClient.ExecutePostAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

    public async Task<ResponseWrapperDto> DeleteCoachAsync(string id, string name)
    {
        var request = new RestRequest($"/api/teams/{id}/coach/{name}");
        var response = await restClient.ExecuteDeleteAsync<ResponseWrapperDto>(request);
        return response.Data!;
    }

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