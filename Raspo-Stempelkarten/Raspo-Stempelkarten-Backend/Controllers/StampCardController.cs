using System.Web;
using DispatchR;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.StampCardCreate;
using Raspo_Stempelkarten_Backend.Commands.StampCardDelete;
using Raspo_Stempelkarten_Backend.Commands.StampCardStamp;
using Raspo_Stempelkarten_Backend.Commands.StampCardStampErase;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.StampCardGetDetails;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/teams/{team}/seasons/{season}/[controller]/")]
public class StampCardController(
    IMediator mediator,
    IValidator<StampCardCreateDto> createDtoValidator) 
    : ControllerBase
{
    [HttpGet]
    public IActionResult List()
    {
        // return Ok("Test List");
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(string team, string season, Guid id)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        var stempelkartenDetailsResult = await mediator.Send(
            new StampCardGetByIdQuery(team, season, id), 
            CancellationToken.None);
        if (stempelkartenDetailsResult.IsFailed) return Problem("Stempelkarte konnte nicht geladen werden!");
        return Ok(stempelkartenDetailsResult.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StampCardCreateDto stampCardCreateDto, 
        string team, string season)
    {
        stampCardCreateDto.Team = HttpUtility.UrlDecode(team);
        stampCardCreateDto.Season = HttpUtility.UrlDecode(season);
        await createDtoValidator.ValidateAsync(stampCardCreateDto);
        
        // create model and perform update
        var response = await mediator.Send(
            new StampCardCreateCommand(stampCardCreateDto), 
            CancellationToken.None);
        return response.IsSuccess ? Ok(response.Value) : Problem("Fehler beim Anlegen der Stempelkarte aufgetreten.");
    }

    [HttpPut]
    public IActionResult Update()
    {
        return Ok("Test Update");
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(string team, string season, Guid id, [FromQuery] ulong? version)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        
        // create model and perform update
        var response = await mediator.Send(
            new StampCardDeleteCommand(team, season, id, version), 
            CancellationToken.None);
        if (response.IsSuccess)
        {
            return Ok();    
        }

        return Problem("Stempelkarte konnte nicht gelöscht werden!");
    }
    
    [HttpPost("{id:guid}/stamp")]
    public async Task<IActionResult> Stamp(string team, string season, Guid id, [FromQuery] string? reason)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        reason = HttpUtility.UrlDecode(reason ?? "");
        
        // create model and perform update
        var response = await mediator.Send(
            new StampCardStampCommand(team, season, id, reason), 
            CancellationToken.None);
        if (response.IsSuccess)
        {
            return Ok(response.Value);    
        }

        return Problem("Stempelkarte konnte nicht gelöscht werden!");
    }
    
    [HttpDelete("{stampCardId:guid}/stamp/{id:guid}")]
    public async Task<IActionResult> Stamp(string team, string season, Guid stampCardId, Guid id)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        
        // create model and perform update
        var response = await mediator.Send(
            new StampCardStampErasedCommand(team, season, stampCardId, id), 
            CancellationToken.None);
        if (response.IsSuccess)
        {
            return Ok(response.Value);    
        }

        return Problem("Stempelkarte konnte nicht gelöscht werden!");
    }
}