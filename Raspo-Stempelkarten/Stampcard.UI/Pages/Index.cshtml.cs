using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;
using Stampcard.UI.Dtos;

namespace Stampcard.UI.Pages;

public class Index(TeamHttpClient teamHttpClient, ILogger<Index> logger) : PageModel
{
    public required List<(string Id, string Name)> Items { get; set; } = [];
    
    [BindProperty] public string Name { get; set; }

    public async Task OnGetAsync()
    {
        var response = await LoadItemsAsync();
        if (response!.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message);
        }
    }

    private async Task<ResponseWrapperDto<List<TeamReadDto>>?> LoadItemsAsync()
    {
        var teamResponse = await teamHttpClient.ListTeamsAsync();
        if (teamResponse.HasError) return teamResponse;
        foreach (var teamReadDto in teamResponse.Data)
        {
            Items.Add((teamReadDto.Id, teamReadDto.Club + "-" + teamReadDto.Name));
        }

        return teamResponse;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var response = await teamHttpClient.CreateTeamAsync(Name);
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
                "Unerwarteter Fehler beim Speichern des Teams. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }

    public async Task<IActionResult> OnGetDelete(string id)
    {
        try
        {
            var response = await teamHttpClient.DeleteTeamAsync(id);
            if (!response.HasError) return RedirectToPage();
            await LoadItemsAsync();
            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unerwarteter Fehler");
            ModelState.AddModelError(
                string.Empty, 
                "Unerwarteter Fehler beim Löschen des Teams. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }
    
    public IActionResult OnGetItem(string target, string id)
    {
        return RedirectToPage(target, new { Team = id });
    }
}