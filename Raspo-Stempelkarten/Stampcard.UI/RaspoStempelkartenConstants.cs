using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raspo.StampCard.Web;

public static class RaspoStempelkartenConstants
{
    public static JsonSerializerOptions RaspoStempelkartenSerializerOptions { get; } =
        new JsonSerializerOptions(JsonSerializerOptions.Default)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

}