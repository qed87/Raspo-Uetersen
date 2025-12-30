using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Queries.Misc;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/[controller]/")]
public class MiscController(IMediator mediator) : ControllerBase
{
    [HttpGet("seasons")]
    public async Task<IActionResult> GetSeasons([FromQuery] bool includeTeams = false)
    {
        var result = await mediator.Send(new GetSeasonsWithTeamsQuery(), CancellationToken.None);
        return includeTeams ? Ok(result) : Ok(result.Seasons.Keys);
    }
    
    [HttpGet("seasons/{season}/teams")]
    public async Task<IActionResult> GetTeamsBySeason(string season)
    {
        var result = await mediator.Send(new GetSeasonsWithTeamsQuery(), CancellationToken.None);
        return Ok(result.Seasons[HttpUtility.UrlDecode(season)].Teams.ToList());
    }
    
    [HttpGet("teams")]
    public async Task<IActionResult> GetAllTeams()
    {
        var result = await mediator.Send(new GetSeasonsWithTeamsQuery(), CancellationToken.None);
        return Ok(result.Seasons.SelectMany(kvp => kvp.Value.Teams).Distinct().ToList());
    }
}