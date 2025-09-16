using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Queries.Teams;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/[controller]/")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await mediator.Send(new GetTeamSeasonQuery(), CancellationToken.None);
        return Ok(result.Teams.Keys.ToList());
    }
    
    [HttpGet("{team}/seasons")]
    public async Task<IActionResult> GetTeamSeasons(string team)
    {
        var result = await mediator.Send(new GetTeamSeasonQuery(), CancellationToken.None);
        return Ok(result.Teams[team].Seasons.ToList());
    }
}