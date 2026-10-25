using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

/// <summary>
/// Evaluation of the accounting: the players of all teams without a stamp card or without enough accepted stamps
/// in an accounting year. The query is passed in the url, so that an evaluation can be reloaded and bookmarked.
/// </summary>
public class Accounting(AccountingHttpClient accountingHttpClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    [Range(2000, 2100, ErrorMessage = "Geschäftsjahr muss zwischen 2000 und 2100 liegen!")]
    public int AccountingYear { get; set; } = DateTime.Today.Year;

    /// <summary>
    /// Not set until the user starts the evaluation.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    [Range(1, 1000, ErrorMessage = "Erforderliche Stempel müssen mindestens 1 sein!")]
    public int? RequiredStamps { get; set; }

    /// <summary>
    /// The result of the evaluation, null as long as none was run.
    /// </summary>
    public List<IncompleteStampCardReadDto>? Items { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!User.IsInRole(ClaimsPrincipalExtensions.AccountingRole)) return RedirectToPage("/Index");
        if (RequiredStamps is null || !ModelState.IsValid) return Page();

        var response = await accountingHttpClient.ListIncompleteStampCardsAsync(AccountingYear, RequiredStamps.Value);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message!);
            return Page();
        }

        Items = response.Data
            .OrderBy(item => item.TeamName)
            .ThenBy(item => item.LastName)
            .ThenBy(item => item.FirstName)
            .ToList();
        return Page();
    }
}
