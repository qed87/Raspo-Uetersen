using System.Web;
using DispatchR;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.StampCardCreate;
using Raspo_Stempelkarten_Backend.Commands.StampCardDelete;
using Raspo_Stempelkarten_Backend.Commands.StampCardStamp;
using Raspo_Stempelkarten_Backend.Commands.StampCardStampErase;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;
using Raspo_Stempelkarten_Backend.Queries.StampCardList;
using Raspo_Stempelkarten_Backend.Queries.StampCardStamp;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/teams/{team}/seasons/{season}/[controller]/")]
public class StampCardController(
    IMediator mediator,
    IValidator<StampCardCreateDto> createDtoValidator) 
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(string team, string season)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        var stempelkartenDetailsResult = await mediator.Send(
            new StampCardListQuery(team, season), 
            CancellationToken.None);
        if (stempelkartenDetailsResult.IsFailed) return Problem("Stempelkarten konnte nicht geladen werden!");
        return Ok(stempelkartenDetailsResult.Value);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(string team, string season, Guid id, bool includeDetails)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        if (includeDetails)
        {
            var stampCardDetailedResult = await mediator.Send(
                new StampCardDetailedGetByIdQuery(team, season, id), 
                CancellationToken.None);
            if (stampCardDetailedResult.IsFailed) return Problem("Stempelkarte konnte nicht geladen werden!");
            return Ok(stampCardDetailedResult.Value);
        }

        var stampCardResult = await mediator.Send(
            new StampCardGetByIdQuery(team, season, id), 
            CancellationToken.None);
        if (stampCardResult.IsFailed) return Problem("Stempelkarte konnte nicht geladen werden!");
        return Ok(stampCardResult.Value);
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
    
    [HttpGet("{id:guid}/stamp")]
    public async Task<IActionResult> ListStamps(string team, string season, Guid id)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        var stampCardReadDtos = await mediator.Send(
            new StampCardStampListQuery(team, season, id), 
            CancellationToken.None);
        if (stampCardReadDtos.IsFailed) return Problem("Stempelkarten konnte nicht geladen werden!");
        return Ok(stampCardReadDtos.Value);
    }
    
    [HttpGet("{stampCardId:guid}/stamp/{id}")]
    public async Task<IActionResult> GetStampById(string team, string season, Guid stampCardId, Guid id)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        var stampCardReadDto = await mediator.Send(
            new StampCardStampGetByIdQuery(team, season, stampCardId, id), 
            CancellationToken.None);
        if (stampCardReadDto.IsFailed) return Problem("Stempelkarten konnte nicht geladen werden!");
        return Ok(stampCardReadDto.Value);
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