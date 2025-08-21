using System.Text.Json;
using System.Text.RegularExpressions;
using FluentValidation;
using KurrentDB.Client;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/[controller]")]
public partial class StempelkartenController(
    KurrentDBClient kurrenDbClient, 
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
    public async Task<IActionResult> Create([FromBody] StampCardCreateDto stampCardCreateDto)
    {
        await createDtoValidator.ValidateAsync(stampCardCreateDto);
        
        // create model and perform update
        var model = await LoadStempelkartenModelAsync(
            stampCardCreateDto.Team, stampCardCreateDto.Season);
        var stempelkarte = model.AddStempelkarte(
            stampCardCreateDto.Recipient, 
            HttpContext.User.Identity?.Name ?? "dbo",
            stampCardCreateDto.AdditionalOwner, 
            stampCardCreateDto.MinStamps, 
            stampCardCreateDto.MaxStamps);
        
        await kurrenDbClient.AppendToStreamAsync(
            $"Stempelkarten-{SpecialCharRegex().Replace(stampCardCreateDto.Team, "_")}-{SpecialCharRegex().Replace(stampCardCreateDto.Season, "_")}", 
            StreamState.Any,
            model.GetChanges());
        
        return Ok(stempelkarte.Id);
    }

    private async Task<StempelkartenAggregate> LoadStempelkartenModelAsync(
        string team, string season)
    {
        var stempelkarten = new StempelkartenAggregate
        {
            Team = team,
            Season = season
        };
        
        var result = kurrenDbClient.ReadStreamAsync(
            Direction.Forwards,
            $"Stempelkarten-{SpecialCharRegex().Replace(team, "_")}-{SpecialCharRegex().Replace(season, "_")}",
            StreamPosition.Start);
        await foreach (var resolvedEvent in result)
        {
            stempelkarten.Replay(resolvedEvent);
        }

        stempelkarten.SetLoaded(result.LastStreamPosition);

        return stempelkarten;
    }

    [HttpPut]
    public IActionResult Update()
    {
        return Ok("Test Update");
    }
    
    [HttpDelete]
    public IActionResult Delete()
    {
        return Ok("Test Delete");
    }
    
    [HttpPost("{id}/stamp")]
    public IActionResult Stamp()
    {
        return Ok("Test Create");
    }

    [GeneratedRegex(@"[\s/]+")]
    private static partial Regex SpecialCharRegex();
}