using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Commands.DeletePlayer;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.GetPlayer;
using Raspo_Stempelkarten_Backend.Queries.ListPlayers;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/teams/{team}/[controller]/")]
public class PlayersController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlayerCreateDto playerCreateDto, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new AddPlayersRequest(team)
            {
                FirstName = playerCreateDto.FirstName,
                Surname = playerCreateDto.Surname,
                Birthdate = playerCreateDto.Birthdate,
                // birthplace
            }, 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
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