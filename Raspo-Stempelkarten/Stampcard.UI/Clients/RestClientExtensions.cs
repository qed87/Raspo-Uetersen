using System.Net;
using System.Text.Json;
using RestSharp;

namespace Stampcard.UI.Clients;

internal static class RestClientExtensions
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Executes the request and returns the response envelope of the backend including its data.
    /// </summary>
    public static async Task<ResponseWrapperDto<T>> SendAsync<T>(this IRestClient restClient, RestRequest request)
    {
        var response = await restClient.ExecuteAsync<ResponseWrapperDto<T>>(request);
        return response.IsSuccessful && response.Data is not null
            ? response.Data
            : ResponseWrapperDto<T>.Fail(ErrorMessageOf(response));
    }

    /// <summary>
    /// Executes the request and returns the response envelope of the backend without its data.
    /// </summary>
    public static async Task<ResponseWrapperDto> SendAsync(this IRestClient restClient, RestRequest request)
    {
        var response = await restClient.ExecuteAsync<ResponseWrapperDto>(request);
        return response.IsSuccessful && response.Data is not null
            ? response.Data
            : ResponseWrapperDto.Fail(ErrorMessageOf(response));
    }

    // Domain errors come as envelope { Message, HasError }, validation errors in Fastify's format { message, ... }.
    // Both carry their text in a message property.
    private static string ErrorMessageOf(RestResponse response)
    {
        var message = TryReadMessage(response.Content);
        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest => $"Ungültige Eingabe: {message}",
            HttpStatusCode.NotFound => message ?? "Nicht gefunden.",
            HttpStatusCode.Conflict => message ?? "Die Daten wurden zwischenzeitlich geändert. Bitte neu laden.",
            HttpStatusCode.Forbidden => message ?? "Keine Berechtigung.",
            _ => message ?? response.ErrorMessage ?? "Unerwarteter Fehler."
        };
    }

    private static string? TryReadMessage(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;
        try
        {
            return JsonSerializer.Deserialize<ResponseWrapperDto>(content, SerializerOptions)?.Message;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
