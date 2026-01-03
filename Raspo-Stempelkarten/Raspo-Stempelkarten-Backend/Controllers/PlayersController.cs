using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddPlayer;
using Raspo_Stempelkarten_Backend.Dtos;

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
                Birthdate = playerCreateDto.Birthdate
            }, 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    [HttpGet]
    public Task<IActionResult> Get(Guid id, string team)
    {
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id:guid}")]
    public Task<IActionResult> Delete(Guid id, string team)
    {
        throw new NotImplementedException();
    }
}