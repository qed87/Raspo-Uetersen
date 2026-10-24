using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

/// <summary>
/// View of a stamp card for external users, who open it with a link containing a share code.
/// They may submit stamps, which are pending until a coach approves them.
/// </summary>
[AllowAnonymous]
public class SharedStampCard(SharedStampCardHttpClient sharedStampCardHttpClient) : PageModel
{
    private const string SubmittedKey = "StampSubmitted";

    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid StampCardId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Code { get; set; }

    [BindProperty]
    public SubmittedStamp NewStamp { get; set; } = new();

    public StampCardReadDto? Item { get; set; }

    /// <summary>
    /// Indicates that the previous request submitted a stamp, so a confirmation is shown.
    /// </summary>
    public bool Submitted { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Submitted = TempData[SubmittedKey] is true;
        await LoadItemAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Code))
        {
            ModelState.AddModelError(string.Empty, "Der Link ist unvollständig.");
            return Page();
        }

        if (ModelState.IsValid)
        {
            var response = await sharedStampCardHttpClient.SubmitStampAsync(
                Team, StampCardId, Code, NewStamp.Reason, NewStamp.IssuerName);
            if (!response.HasError)
            {
                TempData[SubmittedKey] = true;
                return RedirectToPage(new { Team, StampCardId, code = Code });
            }

            ModelState.AddModelError(string.Empty, response.Message!);
        }

        await LoadItemAsync();
        return Page();
    }

    private async Task LoadItemAsync()
    {
        if (string.IsNullOrEmpty(Code))
        {
            ModelState.AddModelError(string.Empty, "Der Link ist unvollständig.");
            return;
        }

        var response = await sharedStampCardHttpClient.GetAsync(Team, StampCardId, Code);
        if (response.HasError)
        {
            // the backend does not tell whether the card or the code is unknown, so neither does the page
            ModelState.AddModelError(string.Empty, "Die Stempelkarte wurde nicht gefunden oder der Link ist nicht mehr gültig.");
            return;
        }

        Item = response.Data;
    }
}

public class SubmittedStamp
{
    [Required(ErrorMessage = "Grund darf nicht leer sein!")]
    public string Reason { get; set; } = "";

    [Required(ErrorMessage = "Name darf nicht leer sein!")]
    [MinLength(2, ErrorMessage = "Name muss mindestens 2 Zeichen lang sein!")]
    public string IssuerName { get; set; } = "";
}
