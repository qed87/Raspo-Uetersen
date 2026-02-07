using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.Contracts.Dtos;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Team(TeamHttpClient  teamHttpClient, ILogger<Team> logger) : PageModel
{
    [BindProperty(Name = "team", SupportsGet = true)]
    [Required]
    public string Id { get; set; }
    
    public TeamDetailedReadDto Item { get; set; }
    
    [BindProperty]
    public string NewName { get; set; }
    
    [BindProperty]
    public ulong SetConcurrencyToken { get; set; }
    
    public List<string> Coaches { get; set; } = [];
    
    [BindProperty]
    [Required]
    [EmailAddress]
    public string NewCoach { get; set; }

    public async Task OnGetAsync()
    {
        await LoadItemsAsync();
    }

    private async Task LoadItemsAsync()
    {
        var response = await teamHttpClient.GetTeamAsync(Id);
        Item = response.Data;
        NewName = Item.Name;
        SetConcurrencyToken = Item.ConcurrencyToken;
        Coaches.AddRange(response.Data.Coaches ?? []);
    }
    
    public async Task<IActionResult> OnPostTeamAsync()
    {
        try
        {
            ModelState.Remove(nameof(NewCoach));
            var response = await teamHttpClient.UpdateTeamAsync(Id, NewName, SetConcurrencyToken);
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
                "Unerwarteter Fehler beim Aktualisieren des Teams. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCoachAsync()
    {
        try
        {
            ModelState.Remove(nameof(NewName));
            var response = await teamHttpClient.CreateCoachAsync(Id, NewCoach);
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