using DispatchR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

namespace Raspo_Stempelkarten_Backend.Controllers;
/// <summary>
/// 
/// </summary>
/// <param name="mediator"></param>
[Route("api/[controller]")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="club"></param>
    /// <param name="birthCohort"></param>
    /// <returns></returns>
    //[Authorize("IsClubManager")]
    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] string club, [FromQuery] short birthCohort)
    {
        var response = await mediator.Send(
            new AddTeamRequest(club, birthCohort, User.Identity.Name), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    //[Authorize("IsClubManager")]
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    //[Authorize("IsCoachOrClubManager")]
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var response = await mediator.Send(
            new ListTeamsQuery(), 
            CancellationToken.None);
        return Ok(response);
    }
}