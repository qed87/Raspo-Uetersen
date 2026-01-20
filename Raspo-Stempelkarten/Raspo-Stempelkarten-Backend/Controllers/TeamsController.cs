using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/[controller]")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] string club, [FromQuery] short birthCohort)
    {
        var response = await mediator.Send(
            new AddTeamRequest(club, birthCohort), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var response = await mediator.Send(
            new DeleteTeamRequest(id), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var response = await mediator.Send(
            new ListTeamsQuery(), 
            CancellationToken.None);
        return Ok(response);
    }
}