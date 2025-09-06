using System.Web;
using DispatchR;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteCreate;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteStamp;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.StempelkarteGetDetails;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/teams/{team}/saisons/{season}/[controller]/")]
public class StempelkartenController(
    IMediator mediator,
    IValidator<StempelkartenCreateDto> createDtoValidator) 
    : ControllerBase
{
    [HttpGet]
    public IActionResult List()
    {
        // return Ok("Test List");
        throw new NotImplementedException();
    }
    
    [HttpGet("{id:guid}")]
    public IActionResult Get(string team, string season, Guid id)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        var stempelkartenDetailsResult = mediator.Send(
            new StempelkartenGetByIdQuery(team, season, id), 
            CancellationToken.None);
        if (stempelkartenDetailsResult.IsFailed) return Problem("Stempelkarte konnte nicht geladen werden!");
        return Ok(stempelkartenDetailsResult.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StempelkartenCreateDto stempelkartenCreateDto, 
        string team, string season)
    {
        stempelkartenCreateDto.Team = HttpUtility.UrlDecode(team);
        stempelkartenCreateDto.Season = HttpUtility.UrlDecode(season);
        await createDtoValidator.ValidateAsync(stempelkartenCreateDto);
        
        // create model and perform update
        var response = await mediator.Send(
            new StempelkartenCreateCommand(stempelkartenCreateDto), 
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
            new StempelkartenDeleteCommand(team, season, id, version), 
            CancellationToken.None);
        if (response.IsSuccess)
        {
            return Ok();    
        }

        return Problem("Stempelkarte konnte nicht gelöscht werden!");
    }
    
    [HttpPost("{id:guid}/stempel")]
    public async Task<IActionResult> Stamp(string team, string season, Guid id, [FromQuery] string? reason)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        reason = HttpUtility.UrlDecode(reason ?? "");
        
        // create model and perform update
        var response = await mediator.Send(
            new StempelkartenStampCommand(team, season, id, reason), 
            CancellationToken.None);
        if (response.IsSuccess)
        {
            return Ok();    
        }

        return Problem("Stempelkarte konnte nicht gelöscht werden!");
    }
}