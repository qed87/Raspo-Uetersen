using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.AddMember;
using Raspo_Stempelkarten_Backend.Commands.RemoveMember;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.GetMember;
using Raspo_Stempelkarten_Backend.Queries.ListMembers;

namespace Raspo_Stempelkarten_Backend.Controllers;

/// <summary>
/// Administrate members.
/// </summary>
/// <param name="mediator"></param>
[Authorize]
[Route("api/teams/{team}/[controller]/")]
public class MembersController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new member.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MemberCreateDto memberCreateDto, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new AddMemberCommand(team, memberCreateDto.FirstName, memberCreateDto.LastName, 
                memberCreateDto.Birthdate, memberCreateDto.Birthplace), 
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
            new ListMembersQuery(team), 
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
            new GetMemberQuery(team, id), 
            CancellationToken.None);
        if (response is null) return NotFound();
        return Ok(ResponseWrapperDto.Ok(response));
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