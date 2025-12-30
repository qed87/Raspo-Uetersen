using System.Text.Json.Serialization;

namespace Raspo_Stempelkarten_Backend.Queries.Misc;

public class SeasonsResult
{
    [JsonPropertyName("seasons")]
    public Dictionary<string, SeasonResult> Seasons { get; set; }
    
}

public class SeasonResult
{
    [JsonPropertyName("teams")]
    public List<string> Teams { get; set; }
}