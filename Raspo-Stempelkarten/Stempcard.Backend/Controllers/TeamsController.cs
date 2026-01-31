using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddCoach;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Commands.ListCoach;
using Raspo_Stempelkarten_Backend.Commands.ListTeamsQuery;
using Raspo_Stempelkarten_Backend.Commands.RemoveCoach;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Controllers;
/// <summary>
/// The teams controller. 
/// </summary>
[Route("api/[controller]")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new team.
    /// </summary>
    /// <param name="club">The club name.</param>
    /// <param name="name">Team name.</param>
    //[Authorize("IsClubManager")]
    [HttpPost]
    public async Task<IActionResult> Create(string club, string name)
    {
        var response = await mediator.Send(
            new AddTeamRequest(club, name, User.Identity?.Name ?? "dbo"), 
            CancellationToken.None);
        
        var httpResponse = response.ToHttpResponse();
        return httpResponse;
    }
    
    /// <summary>
    /// Deletes a team.
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
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// List all available teams.
    /// </summary>
    /// <returns></returns>
    //[Authorize("IsCoachOrClubManager")]
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var response = await mediator.Send(
            new ListTeamsQuery(), 
            CancellationToken.None);
        return Ok(ResponseWrapperDto.Ok(response ?? []));
    }
    
    /// <summary>
    /// List coaches of the team.
    /// </summary>
    [HttpGet("{team}/coach")]
    public async Task<IActionResult> ListCoachesAsync(string team)
    {
        var response = await mediator.Send(
            new ListCoachQuery(team, User.Identity?.Name ?? "dbo"), 
            CancellationToken.None);
        return Ok(ResponseWrapperDto.Ok(response ?? []));
    }
    
    /// <summary>
    /// The coach of this team.
    /// </summary>
    [HttpPost("{team}/coach/{name}")]
    public async Task<IActionResult> CreateCoachAsync(string team, string name)
    {
        var response = await mediator.Send(
            new AddCoach(team, name, User.Identity?.Name ?? "dbo"), 
            CancellationToken.None);
        return Ok(response.ToHttpResponse());
    }
    
    /// <summary>
    /// Delete the coach from the team.
    /// </summary>
    [HttpDelete("{team}/coach/{name}")]
    public async Task<IActionResult> DeleteCoachAsync(string team, string name)
    {
        var response = await mediator.Send(
            new RemoveCoach(team, name, User.Identity?.Name ?? "dbo"), 
            CancellationToken.None);
        return Ok(response.ToHttpResponse());
    }
}