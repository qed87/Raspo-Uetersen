using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StampCard.Backend.Authorization;
using StampCard.Backend.Commands.AddPlayer;
using StampCard.Backend.Commands.RemovePlayer;
using StampCard.Backend.Commands.UpdatePlayer;
using StampCard.Backend.Queries.GetPlayer;
using StampCard.Backend.Queries.ListPlayers;
using Stampcard.Contracts.Dtos;

namespace StampCard.Backend.Controllers;

/// <summary>
/// Administrate members.
/// </summary>
/// <param name="mediator"></param>
[Authorize]
[TeamCoachRequired("team")]
[Route("api/teams/{team}/[controller]/")]
public class PlayersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new member.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(PlayerCreateDto playerCreateDto, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new AddPlayerCommand(team, playerCreateDto.FirstName, playerCreateDto.LastName, 
                playerCreateDto.Birthdate, playerCreateDto.Birthplace), 
                CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// Create a new member.
    /// </summary>
    [HttpPut("{id:Guid}")]
    public async Task<IActionResult> Update(PlayerUpdateDto playerUpdateDto, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new UpdatePlayerCommand(team, playerUpdateDto.Id, playerUpdateDto.FirstName, playerUpdateDto.LastName,
                playerUpdateDto.Birthdate, playerUpdateDto.Birthplace, playerUpdateDto.ConcurrencyToken, playerUpdateDto.Active), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// Lists all member.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(string team)
    {
        team = HttpUtility.UrlDecode(team);
        var responseStream = mediator.CreateStream(
            new ListPlayersQuery(team), 
            CancellationToken.None);
        var players = await responseStream.ToListAsync();
        return Ok(ResponseWrapperDto.Ok(players));
    }
    
    /// <summary>
    /// Get a member.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new GetPlayerQuery(team, id), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// Delete a member.
    /// </summary>
    /// <param name="id">The player id.</param>
    /// <param name="team">The team.</param>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new RemoveMemberCommand(team, id), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
}