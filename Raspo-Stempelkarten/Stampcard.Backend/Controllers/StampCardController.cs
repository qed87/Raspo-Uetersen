using System.Web;
using DispatchR;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StampCard.Backend.Commands.CreateStampCard;
using StampCard.Backend.Commands.DeleteStampCard;
using StampCard.Backend.Commands.EraseStamp;
using StampCard.Backend.Commands.StampStampCard;
using StampCard.Backend.Dtos;
using StampCard.Backend.Queries.GetStampCard;
using StampCard.Backend.Queries.GetStampCardDetails;
using StampCard.Backend.Queries.GetStampsQuery;
using StampCard.Backend.Queries.ListStampCards;

namespace StampCard.Backend.Controllers;

/// <summary>
/// Administrate Stamp Cards.
/// </summary>
/// <param name="mediator"></param>
[Authorize]
[Route("api/teams/{team}/[controller]")]
public class StampCardController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new stamp card.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StampCardCreateDto stampCardCreateDto, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new CreateStampCardCommand(team, stampCardCreateDto.PlayerId, stampCardCreateDto.AccountingYear), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// Stamps a stamp card.
    /// </summary>
    [HttpPost("{id:guid}/stamp")]
    public async Task<IActionResult> Stamp(Guid id, [FromForm] string reason, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new StampStampCardCommand(team, id, reason), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// List all stamps.
    /// </summary>
    [HttpGet("{id:guid}/stamp")]
    public async Task<IActionResult> ListStamps(Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new GetStampsQuery(team, id), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// Erase a stamp.
    /// </summary>
    [HttpDelete("{stampId:guid}/stamp/{id:guid}")]
    public async Task<IActionResult> EraseStamp(Guid stampId, Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new EraseStampCommand(stampId, id, team), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// List stamp cards.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(string team)
    {
        team = HttpUtility.UrlDecode(team);
        var responseStream = mediator.CreateStream(
            new ListStampCardsQuery(team), 
            CancellationToken.None);
        var stampCards = await responseStream.ToListAsync();
        return Ok(ResponseWrapperDto.Ok(stampCards));
    }
    
    /// <summary>
    /// Get stamp card details.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, string team, [FromQuery] bool includeDetails = false)
    {
        team = HttpUtility.UrlDecode(team);
        if (includeDetails)
        {
            var response = await mediator.Send(
                new GetStampCardDetailsQuery(id, team), 
                CancellationToken.None);
            return response.ToHttpResponse();
        } 
        else
        {
            var response = await mediator.Send(
                new GetStampCardQuery(id, team), 
                CancellationToken.None); 
            return response.ToHttpResponse();
        }
    }
    
    /// <summary>
    /// Delete a stamp card.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new DeleteStampCardCommand(id, team), 
            CancellationToken.None);
        return response.ToHttpResponse();
    }
}