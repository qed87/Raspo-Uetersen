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
        var teams = await mediator.Send(new ListTeamsQuery(), CancellationToken.None);
        return Ok(teams);
    }
}