using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Team(TeamHttpClient  teamHttpClient, ILogger<Team> logger) : PageModel
{
    [BindProperty(Name = "team", SupportsGet = true)]
    [Required]
    public string Id { get; set; }
    
    public List<string> Items { get; set; } = [];
    
    [BindProperty]
    [Required]
    [EmailAddress]
    public string Coach { get; set; }

    public async Task OnGetAsync()
    {
        await LoadItemsAsync();
    }

    private async Task LoadItemsAsync()
    {
        var response = await teamHttpClient.ListCoachesAsync(Id);
        Items.AddRange(response.Data ?? []);
    }
    
    public async Task<IActionResult> OnPutAsync()
    {
        try
        {
            //var response = await teamHttpClient.CreateCoachAsync(Id, Coach);
            //if (!response.HasError) return RedirectToPage();
            await LoadItemsAsync();
            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unerwarteter Fehler");
            await LoadItemsAsync();
            ModelState.AddModelError(
                string.Empty, 
                "Unerwarteter Fehler beim Aktualisieren des Teams. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCoachAsync()
    {
        try
        {
            var response = await teamHttpClient.CreateCoachAsync(Id, Coach);
            if (!response.HasError) return RedirectToPage();
            await LoadItemsAsync();
            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unerwarteter Fehler");
            await LoadItemsAsync();
            ModelState.AddModelError(
                string.Empty, 
                "Unerwarteter Fehler beim Speichern eines Coaches. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }
    
    public async Task<IActionResult> OnGetDeleteAsync(string name)
    {
        try
        {
            var response = await teamHttpClient.DeleteCoachAsync(Id, name);
            if (!response.HasError) return RedirectToPage();
            await LoadItemsAsync();
            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unerwarteter Fehler");
            await LoadItemsAsync();
            ModelState.AddModelError(
                string.Empty, 
                "Unerwarteter Fehler beim Löschen eines Coaches. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }
    
}