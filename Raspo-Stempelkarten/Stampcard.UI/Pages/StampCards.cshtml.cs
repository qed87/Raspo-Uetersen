using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class StampCards(StampCardHttpClient stampCardHttpClient, PlayerHttpClient playerHttpClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }

    public List<SelectListItem> Players { get; set; } = [];
    
    public List<StampCard> Items { get; set; } = [];
    
    [BindProperty]
    public string PlayerName { get; set; }

    [BindProperty]
    public int AccountingYear { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadItemsAsync();
        return Page();
    }

    private async Task LoadItemsAsync()
    {
        var playerResponse = await playerHttpClient.ListAsync(Team);
        var stampCardResponse = await stampCardHttpClient.ListAsync(Team);
        bool hasErrors = false;
        if (playerResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, playerResponse.Message!);
            hasErrors = true;
        }

        if (stampCardResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, stampCardResponse.Message!);
            hasErrors = true;
        }
        
        if (hasErrors) return;

        foreach (var playerReadDto in playerResponse.Data)
            Players.Add(new SelectListItem($"{playerReadDto.LastName}, {playerReadDto.FirstName}", Convert.ToString(playerReadDto.Id)));
        
        var playersDict =  playerResponse.Data.ToDictionary(x => x.Id, x => $"{x.LastName}, {x.FirstName}");
        foreach (var stampCardReadDto in stampCardResponse.Data)
        {
            Items.Add(new StampCard
            {
                Id = stampCardReadDto.Id,
                PlayerId = stampCardReadDto.PlayerId,
                PlayerName = playersDict[stampCardReadDto.PlayerId],
                AccountingYear = stampCardReadDto.AccountingYear
            });
        }
    }

    public async Task<IActionResult> OnPostManualAsync(Guid playerName,  int accountingYear)
    {
        var response = await stampCardHttpClient.CreateManualAsync(Team, playerName, accountingYear);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnPostAutoAsync(int accountingYear)
    {
        var response = await stampCardHttpClient.CreateAutoAsync(Team, accountingYear);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnGetDeleteAsync(Guid id)
    {
        var response = await stampCardHttpClient.DeleteAsync(Team, id);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
}

public class StampCard
{
    public Guid Id { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public string PlayerName { get; set; }
    
    public int AccountingYear { get; set; }
    
}