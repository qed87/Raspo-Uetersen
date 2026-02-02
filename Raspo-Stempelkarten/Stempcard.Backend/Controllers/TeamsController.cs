using DispatchR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddCoach;
using Raspo_Stempelkarten_Backend.Commands.AddTeam;
using Raspo_Stempelkarten_Backend.Commands.DeleteTeam;
using Raspo_Stempelkarten_Backend.Commands.RemoveCoach;
using Raspo_Stempelkarten_Backend.Commands.UpdateTeam;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.GetTeam;
using Raspo_Stempelkarten_Backend.Queries.ListCoach;
using Raspo_Stempelkarten_Backend.Queries.ListTeamsQuery;

namespace Raspo_Stempelkarten_Backend.Controllers;
/// <summary>
/// The teams controller. 
/// </summary>
[Authorize]
[Route("api/[controller]")]
public class TeamsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Get the team.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetItemAsync(string id)
    {
        var response = await mediator.Send(
            new GetTeamQuery(id), 
            CancellationToken.None);
        return Ok(ResponseWrapperDto.Ok(response!));
    }
    
    /// <summary>
    /// Create a new team.
    /// </summary>
    /// <param name="club">The club name.</param>
    /// <param name="name">Team name.</param>
    [Authorize("IsManager")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync(string club, string name)
    {
        var response = await mediator.Send(
            new AddTeamCommand(club, name), 
            CancellationToken.None);
        var httpResponse = response.ToHttpResponse();
        return httpResponse;
    }
    
    /// <summary>
    /// Create a new team.
    /// </summary>
    /// <param name="id">The stream id.</param>
    /// <param name="name">The new team name.</param>
    /// <param name="concurrencyToken">The concurrency token.</param>
    [Authorize("IsManager")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(string id, string name, ulong concurrencyToken)
    {
        var response = await mediator.Send(
            new UpdateTeamCommand(id, name, concurrencyToken), 
            CancellationToken.None);
        var httpResponse = response.ToHttpResponse();
        return httpResponse;
    }
    
    /// <summary>
    /// Deletes a team.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize("IsManager")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var response = await mediator.Send(
            new DeleteTeamCommand(id), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// List all available teams.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> ListAsync()
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
            new ListCoachQuery(team), 
            CancellationToken.None);
        return Ok(ResponseWrapperDto.Ok(response ?? []));
    }
    
    /// <summary>
    /// The coach of this team.
    /// </summary>
    [Authorize("IsManager")]
    [HttpPost("{team}/coach")]
    public async Task<IActionResult> CreateCoachAsync(string team, string name)
    {
        var response = await mediator.Send(
            new AddCoachCommand(team, name), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// Delete the coach from the team.
    /// </summary>
    [Authorize("IsManager")]
    [HttpDelete("{team}/coach/{name}")]
    public async Task<IActionResult> DeleteCoachAsync(string team, string name)
    {
        var response = await mediator.Send(
            new RemoveCoachCommand(team, name), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
}