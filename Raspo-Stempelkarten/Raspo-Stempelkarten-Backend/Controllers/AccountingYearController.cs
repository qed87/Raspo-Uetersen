using System.Web;
using DispatchR;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;
using Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/teams/{team}/accounting-year")]
public class AccountingYearController(IMediator mediator) : ControllerBase
{
    [HttpPost("{accountingYear:int}/create")]
    public async Task<IActionResult> CreateFiscalYear(int accountingYear, string team)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new CreateTeamStampCardsForAccountingYears(team, accountingYear),
            CancellationToken.None);
        return response.IsFailed 
            ? Problem(string.Join(Environment.NewLine, response.Errors.Select(e => e.Message))) 
            : Ok(response.Value);
    }
    
    [HttpGet("{accountingYear:int}/stampcard/incompleted")]
    public async Task<IActionResult> GetIncompletedStampCards(
        int accountingYear, 
        string team,
        [FromQuery(Name = "numberOfRequiredStamps")] int numberOfRequiredStamps)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new GetIncompletedStampCardsQuery(team, accountingYear) { NumberOfRequiredStamps = numberOfRequiredStamps },
            CancellationToken.None);
        if (response is null) return NotFound();
        return Ok(response);
    }
    
    [HttpGet("{accountingYear:int}/stampcard/completed")]
    public async Task<IActionResult> GetCompletedStampCards(
        int accountingYear, 
        string team,
        [FromQuery(Name = "numberOfRequiredStamps")] int numberOfRequiredStamps)
    {
        team = HttpUtility.UrlDecode(team);
        var response = await mediator.Send(
            new GetCompletedStampCardsQuery(team, accountingYear) { NumberOfRequiredStamps = numberOfRequiredStamps },
            CancellationToken.None);
        if (response is null) return NotFound();
        return Ok(response);
    }
}