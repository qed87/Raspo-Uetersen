using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Index(TeamHttpClient teamHttpClient, StampCardHttpClient stampCardHttpClient, ILogger<Index> logger) : PageModel
{
    public required List<TeamReadDto> Items { get; set; } = [];

    /// <summary>
    /// The number of stamps waiting for approval per team id, only for teams the user coaches.
    /// </summary>
    public Dictionary<string, int> PendingStampCounts { get; set; } = [];
    
    [BindProperty] public string Name { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var response = await LoadItemsAsync();
        if (response!.HasError)
            ModelState.AddModelError(string.Empty, response.Message!);
        return Page();
    }

    private async Task<ResponseWrapperDto<List<TeamReadDto>>?> LoadItemsAsync()
    {
        var teamResponse = await teamHttpClient.ListTeamsAsync();
        if (teamResponse.HasError) return teamResponse;
        foreach (var teamReadDto in teamResponse.Data)
        {
            Items.Add(teamReadDto);
            if (!User.IsCoachOf(teamReadDto)) continue;
            // a missing count only hides the badge, the overview stays usable
            var pendingResponse = await stampCardHttpClient.ListPendingStampsAsync(teamReadDto.Id);
            if (!pendingResponse.HasError) PendingStampCounts[teamReadDto.Id] = pendingResponse.Data.Count;
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
            ModelState.AddModelError(string.Empty, response.Message!);
            return Page();
        }
        catch (Exception exception) when (exception is not UnauthorizedAccessException)
        {
            logger.LogError(exception, "Unerwarteter Fehler");
            await LoadItemsAsync();
            ModelState.AddModelError(
                string.Empty, 
                "Unerwarteter Fehler beim Speichern des Teams. Bitte informieren Sie Ihren Systemadministrator.");
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        // the name of a new team is not part of this form
        ModelState.Clear();
        try
        {
            var response = await teamHttpClient.DeleteTeamAsync(id);
            if (!response.HasError) return RedirectToPage();
            await LoadItemsAsync();
            ModelState.AddModelError(string.Empty, response.Message);
            return Page();
        }
        catch (Exception exception) when (exception is not UnauthorizedAccessException)
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