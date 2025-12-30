using System.Web;
using DispatchR;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.StampCardCreate;
using Raspo_Stempelkarten_Backend.Commands.StampCardDelete;
using Raspo_Stempelkarten_Backend.Commands.StampCardStamp;
using Raspo_Stempelkarten_Backend.Commands.StampCardStampErase;
using Raspo_Stempelkarten_Backend.Commands.StampCardUpdate;
using Raspo_Stempelkarten_Backend.Dtos;
using Raspo_Stempelkarten_Backend.Queries.StampCardGetDetailed;
using Raspo_Stempelkarten_Backend.Queries.StampCardList;
using Raspo_Stempelkarten_Backend.Queries.StampCardStamp;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/seasons/{season}/teams/{team}/[controller]/")]
public class StampCardController(
    IMediator mediator,
    IValidator<StampCardCreateDto> createDtoValidator,
    IValidator<StampCardUpdateDto> updateDtoValidator) 
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(string season, string team)
    {
        season = HttpUtility.UrlDecode(season);
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new StampCardListQuery(season, team), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(string season, string team, Guid id, bool includeDetails)
    {
        season = HttpUtility.UrlDecode(season);
        team = HttpUtility.UrlDecode(team);
        if (includeDetails)
        {
            var detailedResponse = await mediator.Send(
                new StampCardDetailedGetByIdQuery(season, team, id), 
                CancellationToken.None);
            return detailedResponse.IsFailed 
                ? Problem(string.Join(Environment.NewLine, detailedResponse.Errors.Select(error => error.Message))) 
                : Ok(detailedResponse.Value);
        }

        var response = await mediator.Send(
            new StampCardGetByIdQuery(season, team, id), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] StampCardCreateDto stampCardCreateDto, 
        string team, 
        string season)
    {
        stampCardCreateDto.Season = HttpUtility.UrlDecode(season);
        stampCardCreateDto.Team = HttpUtility.UrlDecode(team);
        var validationResult = await createDtoValidator.ValidateAsync(stampCardCreateDto);
        if (validationResult.Errors.Count != 0) 
            return Problem(string.Join(Environment.NewLine, validationResult.Errors.Select(failure => failure.ErrorMessage)));
        var response = await mediator.Send(
            new StampCardCreateCommand(stampCardCreateDto), 
            CancellationToken.None);
        return response.IsSuccess 
            ? Ok(response.Value) 
            : Problem(string.Join(Environment.NewLine, response.Errors.Select(error => error.Message)));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromBody] StampCardUpdateDto stampCardUpdateDto,
        Guid id,
        string team, 
        string season)
    {
        stampCardUpdateDto.Id = id;
        stampCardUpdateDto.Season = HttpUtility.UrlDecode(season);
        stampCardUpdateDto.Team = HttpUtility.UrlDecode(team);
        var validationResult = await updateDtoValidator.ValidateAsync(stampCardUpdateDto);
        if (validationResult.Errors.Count != 0) 
            return Problem(string.Join(Environment.NewLine, validationResult.Errors.Select(failure => failure.ErrorMessage)));
        var response = await mediator.Send(
            new StampCardUpdateCommand(stampCardUpdateDto), 
            CancellationToken.None);
        return response.IsSuccess 
            ? Ok(response.Value) 
            : Problem(string.Join(Environment.NewLine, response.Errors.Select(error => error.Message)));
    }

    [HttpPut]
    public IActionResult Update()
    {
        return Ok("Test Update");
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(string season, string team, Guid id, [FromQuery] ulong? version)
    {
        season = HttpUtility.UrlDecode(season);
        team = HttpUtility.UrlDecode(team);
        
        // create model and perform update
        var response = await mediator.Send(
            new StampCardDeleteCommand(season, team, id, version), 
            CancellationToken.None);
        if (response.IsSuccess)
        {
            return Ok();    
        }

        return Problem(string.Join(Environment.NewLine, response.Errors.Select(error => error.Message)));
    }
    
    [HttpGet("{id:guid}/stamp")]
    public async Task<IActionResult> ListStamps(string season, string team, Guid id)
    {
        season = HttpUtility.UrlDecode(season);
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new StampCardStampListQuery(season, team, id), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(error => error.Message))) 
            : Ok(response.Value);
    }
    
    [HttpGet("{stampCardId:guid}/stamp/{id}")]
    public async Task<IActionResult> GetStampById(string season, string team, Guid stampCardId, Guid id)
    {
        season = HttpUtility.UrlDecode(season);
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new StampCardStampGetByIdQuery(season, team, stampCardId, id), 
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(error => error.Message))) 
            : Ok(response.Value);
    }
    
    [HttpPost("{id:guid}/stamp")]
    public async Task<IActionResult> Stamp(string season, string team, Guid id, [FromQuery] string? reason)
    {
        season = HttpUtility.UrlDecode(season);
        team = HttpUtility.UrlDecode(team);
        reason = HttpUtility.UrlDecode(reason ?? "");
        
        // create model and perform update
        var response = await mediator.Send(
            new StampCardStampCommand(season, team, id, reason), 
            CancellationToken.None);
        return response.IsSuccess 
            ? Ok(response.Value) 
            : Problem(string.Join(Environment.NewLine, response.Errors.Select(error => error.Message)));
    }
    
    [HttpDelete("{stampCardId:guid}/stamp/{id:guid}")]
    public async Task<IActionResult> Stamp(string season, string team, Guid stampCardId, Guid id)
    {
        season = HttpUtility.UrlDecode(season);
        team = HttpUtility.UrlDecode(team);
        
        // create model and perform update
        var response = await mediator.Send(
            new StampCardStampErasedCommand(season, team, stampCardId, id), 
            CancellationToken.None);
        return response.IsSuccess 
            ? Ok(response.Value) 
            : Problem(string.Join(Environment.NewLine, response.Errors.Select(error => error.Message)));
    }
}