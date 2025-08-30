using System.Web;
using FluentValidation;
using LiteBus.Commands.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteCreate;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteDelete;
using Raspo_Stempelkarten_Backend.Commands.StempelkarteStamp;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/team/{team}/saison/{season}/[controller]/")]
public class StempelkartenController(
    ICommandMediator commandMediator,
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
    public IActionResult Get(Guid id)
    {
        //return Ok("Test Get");
        throw new NotImplementedException();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] StampCardCreateDto stampCardCreateDto, string team, string season)
    {
        stampCardCreateDto.Team = HttpUtility.UrlDecode(team);
        stampCardCreateDto.Season = HttpUtility.UrlDecode(season);
        await createDtoValidator.ValidateAsync(stampCardCreateDto);
        
        // create model and perform update
        var response = await commandMediator.SendAsync(new StempelkartenCreateCommand(stampCardCreateDto));
        if (response.IsSuccess)
        {
            return Ok(response.Value);    
        }

        return Problem("Fehler beim Anlegen der Stempelkarte aufgetreten.");
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
        var response = await commandMediator.SendAsync(new StempelkartenDeleteCommand(team, season, id, version));
        if (response.IsSuccess)
        {
            return Ok();    
        }

        return Problem("Stempelkarte konnte nicht gelöscht werden!");
    }
    
    [HttpPost("{id:guid}/stamp")]
    public async Task<IActionResult> Stamp(string team, string season, Guid id)
    {
        team = HttpUtility.UrlDecode(team);
        season = HttpUtility.UrlDecode(season);
        
        // create model and perform update
        var response = await commandMediator.SendAsync(new StempelkartenStampCommand(team, season, id));
        if (response.IsSuccess)
        {
            return Ok();    
        }

        return Problem("Stempelkarte konnte nicht gelöscht werden!");
    }
}