using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

/// <summary>
/// Read only view of a stamp card for external users, who open it with a link containing a share code.
/// </summary>
[AllowAnonymous]
public class SharedStampCard(SharedStampCardHttpClient sharedStampCardHttpClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid StampCardId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Code { get; set; }

    public StampCardReadDto? Item { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(Code))
        {
            ModelState.AddModelError(string.Empty, "Der Link ist unvollständig.");
            return Page();
        }

        var response = await sharedStampCardHttpClient.GetAsync(Team, StampCardId, Code);
        if (response.HasError)
        {
            // the backend does not tell whether the card or the code is unknown, so neither does the page
            ModelState.AddModelError(string.Empty, "Die Stempelkarte wurde nicht gefunden oder der Link ist nicht mehr gültig.");
            return Page();
        }

        Item = response.Data;
        return Page();
    }
}
