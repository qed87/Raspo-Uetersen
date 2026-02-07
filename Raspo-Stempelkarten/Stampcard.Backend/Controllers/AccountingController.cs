using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StampCard.Backend.Commands.CreateTeamStampCardsForAccountingYear;
using StampCard.Backend.Queries.GetCompletedStampCardsQuery;
using StampCard.Backend.Queries.GetIncompletedStampCardsQuery;

namespace StampCard.Backend.Controllers;

/// <summary>
/// Accounting service
/// </summary>
[Authorize]
[Route("api/teams/{team}/[controller]")]
public class AccountingController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// 
    /// </summary>
    [HttpPost("{year:int}")]
    public async Task<IActionResult> CreateFiscalYear(int year, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new CreateTeamStampCardsForAccountingYearsCommand(team, year),
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    /// <summary>
    /// 
    /// </summary>
    [HttpGet("{year:int}/stampcard")]
    public async Task<IActionResult> GetStampCardsByType(
        int year, 
        string team,
        [FromQuery(Name = "type")] string commandType,
        [FromQuery(Name = "numberOfRequiredStamps")] int numberOfRequiredStamps)
    {
        team = HttpUtility.UrlDecode(team);
        return commandType switch
        {
            "completed" => await GetCompletedStampCardsAsync(team, year, numberOfRequiredStamps),
            "incompleted" => await GetIncompletedStampCardsAsync(team, year, numberOfRequiredStamps),
            _ => throw new NotSupportedException(commandType + " not supported")
        };
    }

    private async Task<IActionResult> GetIncompletedStampCardsAsync(string team, int year, int numberOfRequiredStamps)
    {
        var command = new GetIncompletedStampCardsQuery(team, year, numberOfRequiredStamps);
        var response = await mediator.Send(
            command,
            CancellationToken.None);
        return response.ToHttpResponse();
    }
    
    private async Task<IActionResult> GetCompletedStampCardsAsync(string team, int year, int numberOfRequiredStamps)
    {
        var command = new GetCompletedStampCardsQuery(team, year, numberOfRequiredStamps);
        var response = await mediator.Send(
            command,
            CancellationToken.None);
        return response.ToHttpResponse();
    }
}