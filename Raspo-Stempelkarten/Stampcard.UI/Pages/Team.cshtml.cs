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
    
    public TeamReadDto? Item { get; set; }

    [BindProperty]
    [MinLength(2, ErrorMessage = "Team Name darf nicht leer sein!")]
    public string NewName { get; set; }

    [BindProperty]
    public long SetConcurrencyToken { get; set; }
    
    public List<string> Coaches { get; set; } = [];
    
    [BindProperty]
    [Required(ErrorMessage = "E-Mail darf nicht leer sein!")]
    [EmailAddress(ErrorMessage = "Ungültige E-Mail Adresse!")]
    public string NewCoach { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadItemsAsync();
        return Page();
    }

    private async Task LoadItemsAsync()
    {
        var response = await teamHttpClient.GetTeamAsync(Id);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message!);
            return;
        }

        Item = response.Data;
        NewName = Item.Name;
        SetConcurrencyToken = response.ConcurrencyToken!.Value;
        // render the reloaded token instead of the posted one, otherwise an outdated token would be sent again
        ModelState.Remove(nameof(SetConcurrencyToken));
        Coaches.AddRange(Item.Coaches);
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
        catch (Exception exception) when (exception is not UnauthorizedAccessException)
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
        catch (Exception exception) when (exception is not UnauthorizedAccessException)
        {
            logger.LogError(exception, "Unerwarteter Fehler");
            await LoadItemsAsync();
            ModelState.AddModelError(
                string.Empty, 
                "Unerwarteter Fehler beim Speichern eines Coaches. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(string name)
    {
        // neither the team name nor a new coach are part of this form
        ModelState.Clear();
        try
        {
            var response = await teamHttpClient.DeleteCoachAsync(Id, name);
            if (!response.HasError) return RedirectToPage();
            await LoadItemsAsync();
            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }
        catch (Exception exception) when (exception is not UnauthorizedAccessException)
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