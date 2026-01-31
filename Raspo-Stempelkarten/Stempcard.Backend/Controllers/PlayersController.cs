using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Commands.DeletePlayer;
using Raspo_Stempelkarten_Backend.Commands.GetPlayer;
using Raspo_Stempelkarten_Backend.Commands.ListPlayers;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Controllers;

/// <summary>
/// Administrate players.
/// </summary>
/// <param name="mediator"></param>
[Route("api/teams/{team}/[controller]/")]
public class PlayersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new player.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlayerCreateDto playerCreateDto, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new AddPlayersRequest(team)
            {
                FirstName = playerCreateDto.FirstName,
                LastName = playerCreateDto.LastName,
                Birthdate = playerCreateDto.Birthdate,
                Birthplace = playerCreateDto.Birthplace
            }, 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// Lists all players.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(string team)
    {
        team = HttpUtility.UrlDecode(team);
        var responseStream = mediator.CreateStream(
            new ListPlayersQuery(team), 
            CancellationToken.None);
        var players = await responseStream.ToListAsync();
        return Ok(players);
    }
    
    /// <summary>
    /// Get a Player.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new GetPlayersQuery(team) { Id = id }, 
            CancellationToken.None);
        if (response is null) return NotFound();
        return Ok(response);
    }
    
    /// <summary>
    /// Delete a player.
    /// </summary>
    /// <param name="id">The player id.</param>
    /// <param name="team">The team.</param>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new DeletePlayerRequest(id, team), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
}