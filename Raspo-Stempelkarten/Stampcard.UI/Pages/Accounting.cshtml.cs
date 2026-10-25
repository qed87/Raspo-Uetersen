using System.ComponentModel.DataAnnotations;
using System.Text;
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

        await LoadItemsAsync();
        return Page();
    }

    /// <summary>
    /// The same evaluation as a CSV file. Separated by semicolons and with a byte order mark,
    /// so that Excel opens it with German settings and umlauts.
    /// </summary>
    public async Task<IActionResult> OnGetCsvAsync()
    {
        if (!User.IsInRole(ClaimsPrincipalExtensions.AccountingRole)) return RedirectToPage("/Index");
        if (RequiredStamps is null || !ModelState.IsValid) return Page();

        await LoadItemsAsync();
        if (Items is null) return Page();

        var csv = new StringBuilder();
        csv.AppendLine("Team;Nachname;Vorname;Bestätigte Stempel;Erforderliche Stempel;Status;Deaktiviert am");
        foreach (var item in Items)
        {
            // disabled players collect no stamps, so no stamps are required of them
            var isDisabled = item.DisabledOn is not null;
            string[] fields =
            [
                item.TeamName,
                item.LastName ?? "(Spieler gelöscht)",
                item.FirstName ?? "",
                isDisabled && item.StampCardId is null ? "" : item.AcceptedStamps.ToString(),
                isDisabled ? "" : RequiredStamps.Value.ToString(),
                isDisabled ? "Deaktiviert" : item.StampCardId is null ? "Keine Stempelkarte" : "Zu wenige Stempel",
                isDisabled ? DisplayFormat.Date(item.DisabledOn!.Value) : ""
            ];
            csv.AppendLine(string.Join(';', fields.Select(EscapeCsvField)));
        }

        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(content, "text/csv; charset=utf-8", $"Auswertung-{AccountingYear}.csv");
    }

    private async Task LoadItemsAsync()
    {
        var response = await accountingHttpClient.ListIncompleteStampCardsAsync(AccountingYear, RequiredStamps!.Value);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message!);
            return;
        }

        Items = response.Data
            .OrderBy(item => item.TeamName)
            .ThenBy(item => item.LastName)
            .ThenBy(item => item.FirstName)
            .ToList();
    }

    private static string EscapeCsvField(string field)
    {
        return field.IndexOfAny([';', '"', '\n', '\r']) < 0 ? field : $"\"{field.Replace("\"", "\"\"")}\"";
    }
}
