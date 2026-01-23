using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Raspo.StampCard.Web.Dtos;

[UsedImplicitly]
public class SeasonResponseDto
{
    [JsonPropertyName("seasons")]
    public Dictionary<string, SeasonDto> Seasons { get; set; }

    public class SeasonDto
    {
        [JsonPropertyName("teams")]
        public List<string> Teams { get; set; }
    }
}