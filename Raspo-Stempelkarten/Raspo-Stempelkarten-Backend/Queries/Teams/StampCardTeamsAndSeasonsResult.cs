using System.Text.Json.Serialization;

namespace Raspo_Stempelkarten_Backend.Queries.Teams;

public class StampCardTeamsAndSeasonsResult
{
    [JsonPropertyName("teams")]
    public Dictionary<string, TeamResult> Teams { get; set; }
    
}

public class TeamResult
{
    [JsonPropertyName("seasons")]
    public List<string> Seasons { get; set; }
}