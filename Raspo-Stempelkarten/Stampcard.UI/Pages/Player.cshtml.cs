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
            Player.Birthdate, Player.Birthplace, Player.Active, Player.ConcurrencyToken);
        if (!response.HasError) return RedirectToPage("Players", new {Team = Team });
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemAsync();
        return Page();
    }

    public async Task OnGetAsync()
    {
        await LoadItemAsync();
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
            Birthplace = response.Data.Birthplace,
            Active = response.Data.Active,
            ConcurrencyToken = response.Data.ConcurrencyToken
        };
    }
}