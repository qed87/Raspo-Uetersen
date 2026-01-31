using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.CreateStampCard;
using Raspo_Stempelkarten_Backend.Commands.DeleteStampCard;
using Raspo_Stempelkarten_Backend.Commands.EraseStamp;
using Raspo_Stempelkarten_Backend.Commands.GetStampCard;
using Raspo_Stempelkarten_Backend.Commands.GetStampCardDetails;
using Raspo_Stempelkarten_Backend.Commands.ListStampCards;
using Raspo_Stempelkarten_Backend.Commands.StampStampCard;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Controllers;

/// <summary>
/// Administrate Stamp Cards.
/// </summary>
/// <param name="mediator"></param>
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
            new CreateStampCard(team)
            {
                AccountingYear = stampCardCreateDto.AccountingYear,
                IssuedTo = stampCardCreateDto.IssuedTo
            }, CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    /// <summary>
    /// Stamps a stamp card.
    /// </summary>
    [HttpPost("{id:guid}/stamp")]
    public async Task<IActionResult> Stamp(Guid id, [FromForm] string reason, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new StampStampCard(team, id)
            {
                Reason = reason,
            }, CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    /// <summary>
    /// Erase a stamp.
    /// </summary>
    [HttpDelete("{stampId:guid}/stamp/{id:guid}")]
    public async Task<IActionResult> EraseStamp(Guid stampId, Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new EraseStamp(stampId, id, team), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
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
        return Ok(stampCards);
    }
    
    /// <summary>
    /// Get stamp card details.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, string team, [FromQuery] bool includeDetails = false)
    {
        team = HttpUtility.UrlDecode(team);
        object? response;
        if (includeDetails)
        {
            response = await mediator.Send(
                new GetStampCardDetailsQuery(id, team), 
                CancellationToken.None);    
        } 
        else
        {
            response = await mediator.Send(
                new GetStampCardQuery(id, team), 
                CancellationToken.None); 
        }
        
        if (response is null) return NotFound();
        return Ok(response);
    }
    
    /// <summary>
    /// Delete a stamp card.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new DeleteStampCard(id, team), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
}