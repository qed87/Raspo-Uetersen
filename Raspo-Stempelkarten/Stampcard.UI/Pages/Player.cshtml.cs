using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class PlayerModel(PlayerHttpClient playerHttpClient) : PageModel
{
    [BindProperty] public Player Player { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty(Name = "Id", SupportsGet = true)]
    public Guid PlayerId { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var response = await playerHttpClient.UpdateAsync(Team, PlayerId, Player.FirstName, Player.LastName, 
            Player.Birthdate, Player.ConcurrencyToken);
        if (!response.HasError && Player.Active != Player.WasActive)
            response = await playerHttpClient.SetActiveAsync(Team, PlayerId, Player.Active);
        if (!response.HasError) return RedirectToPage("Players", new {Team = Team });
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadItemAsync();
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Index");
        }
    }

    private async Task LoadItemAsync()
    {
        var response = await playerHttpClient.GetAsync(Team, PlayerId);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message!);
            return;
        }

        Player = new Player
        {
            Id = response.Data.Id,
            FirstName = response.Data.FirstName,
            LastName = response.Data.LastName,
            Birthdate = response.Data.Birthdate,
            Active = response.Data.Active,
            WasActive = response.Data.Active,
            ConcurrencyToken = response.ConcurrencyToken!.Value
        };
        // render the reloaded state instead of the posted one, otherwise an outdated token would be sent again
        ModelState.Remove("Player.ConcurrencyToken");
        ModelState.Remove("Player.WasActive");
    }
}