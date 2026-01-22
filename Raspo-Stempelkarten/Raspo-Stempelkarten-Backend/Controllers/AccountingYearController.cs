using System.Web;
using DispatchR;
using DispatchR.Abstractions.Send;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Commands.CreateTeamStampCardsForAccountingYear;
using Raspo_Stempelkarten_Backend.Queries.GetCompletedStampCardsQuery;
using Raspo_Stempelkarten_Backend.Queries.GetIncompletedStampCardsQuery;

namespace Raspo_Stempelkarten_Backend.Controllers;

[Route("api/teams/{team}/accounting-year")]
public class AccountingYearController(IMediator mediator) : ControllerBase
{
    [HttpPost("{accountingYear:int}")]
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
    
    [HttpGet("{accountingYear:int}/stampcard")]
    public async Task<IActionResult> GetIncompletedStampCards(
        int accountingYear, 
        string team,
        [FromQuery(Name = "type")] string commandType,
        [FromQuery(Name = "numberOfRequiredStamps")] int numberOfRequiredStamps)
    {
        team = HttpUtility.UrlDecode(team);
        dynamic command = commandType switch
        {
            "completed" => new GetCompletedStampCardsQuery(team, accountingYear) { NumberOfRequiredStamps = numberOfRequiredStamps },
            "incompleted" => new GetIncompletedStampCardsQuery(team, accountingYear)
                { NumberOfRequiredStamps = numberOfRequiredStamps },
            _ => throw new NotSupportedException(commandType + " not supported")
        };
        var response = await mediator.Send(
            command,
            CancellationToken.None);
        if (response is null) return NotFound();
        return Ok(response);
    }
}