using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Queries.Teams;

public class StampCardTeamsAndSeasonsResult
{
    public List<string> Teams { get; set; }
    
    public List<string> Seasons { get; set; }
}