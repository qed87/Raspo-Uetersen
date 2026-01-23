using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using FluentResults;
using Raspo.StampCard.Web.Dtos;

namespace Raspo.StampCard.Web.Services;

public class StampCardService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    public async Task<Result> CreateStampCard<T>(string season, string team, T body)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var httpClient = httpClientFactory.CreateClient();
        using var response = await httpClient.PostAsync(
            new Uri(baseUri, $"/api/seasons/{season}/teams/{team}/stampcard"),
            new StringContent(
                JsonSerializer.Serialize(
                    body, 
                    RaspoStempelkartenConstants.RaspoStempelkartenSerializerOptions), 
                Encoding.UTF8, 
                new MediaTypeHeaderValue("application/json")));
        if (response.IsSuccessStatusCode) return Result.Ok();
        var responseText = await response.Content.ReadAsStringAsync();
        var errorMessageJsonObject = JsonSerializer.Deserialize<JsonObject>(responseText);
        var errorMessage = errorMessageJsonObject!["detail"]!.GetValue<string>();
        return Result.Fail(errorMessage);
    }
    
    public async Task<StampCardReadDto?> GetStampCardById(string season, string team, Guid id)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var httpClient = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(baseUri, $"/api/seasons/{season}/teams/{team}/stampcard/{id}"));
        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<StampCardReadDto>(
            responseStream, 
            RaspoStempelkartenConstants.RaspoStempelkartenSerializerOptions);
    }

    public async Task<List<StampReadDto>?> GetStamps(string season, string team, Guid stampCardId)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var httpClient = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(baseUri, $"/api/seasons/{season}/teams/{team}/stampcard/{stampCardId}/stamp"));
        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<List<StampReadDto>>(
            responseStream, 
            RaspoStempelkartenConstants.RaspoStempelkartenSerializerOptions);
    }

    public async Task<bool> DeleteStampCard(string season, string team, Guid id)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var httpClient = httpClientFactory.CreateClient();
        var response = await httpClient.DeleteAsync(
            new Uri(baseUri, $"api/seasons/{season}/teams/{team}/stampcard/{id}"));
        return response.IsSuccessStatusCode;
    }

    public async Task<List<StampCardReadDto>?> ListStampCards(string season, string team)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(baseUri, $"/api/seasons/{season}/teams/{team}/stampcard"));
        using var httpClient = httpClientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<List<StampCardReadDto>>(
            responseStream, 
            RaspoStempelkartenConstants.RaspoStempelkartenSerializerOptions);
    }

    public async Task<SeasonResponseDto?> GetSeasonsWithTeams()
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var seasonsWithTeamsRequest = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(baseUri, "/api/misc/seasons?includeTeams=true"));
        using var httpClient = httpClientFactory.CreateClient();
        using var seasonWithTeamsResponse = await httpClient.SendAsync(seasonsWithTeamsRequest);
        if (!seasonWithTeamsResponse.IsSuccessStatusCode) return null;
        await using var responseStream = await seasonWithTeamsResponse.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<SeasonResponseDto>(responseStream);
    }

    public async Task<bool> DeleteStamp(string season, string team, Guid stampId, Guid id)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var httpClient = httpClientFactory.CreateClient();
        using var response = await httpClient.DeleteAsync(
            new Uri(
                baseUri, 
                $"/api/seasons/{season}/teams/{team}/stampcard/{stampId}/stamp/{id}"));
        if (!response.IsSuccessStatusCode) return false;
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        return true;
    }

    public async Task<StampReadDto?> AddStamp(string season, string team, Guid id, string reason)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var httpClient = httpClientFactory.CreateClient();
        using var response = await httpClient.PostAsync(
            new Uri(baseUri, $"/api/seasons/{season}/teams/{team}/stampcard/{id}/stamp?reason={HttpUtility.UrlEncode(reason)}"), 
            null);
        if (!response.IsSuccessStatusCode) return null;
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<StampReadDto>(
            responseStream, 
            RaspoStempelkartenConstants.RaspoStempelkartenSerializerOptions);
    }

    public async Task<Result> UpdateStamp<T>(Guid id, string season, string team, T body)
    {
        var baseUri = new Uri(configuration.GetConnectionString("Backend")!);
        using var httpClient = httpClientFactory.CreateClient();
        using var response = await httpClient.PutAsJsonAsync(
            new Uri(baseUri, $"/api/seasons/{season}/teams/{team}/stampcard/{id}"),
            body);
        if (response.IsSuccessStatusCode) return Result.Ok();
        var responseText = await response.Content.ReadAsStringAsync();
        var errorMessageJsonObject = JsonSerializer.Deserialize<JsonObject>(responseText);
        var errorMessage = errorMessageJsonObject!["detail"]!.GetValue<string>();
        return Result.Fail(errorMessage);
    }
}