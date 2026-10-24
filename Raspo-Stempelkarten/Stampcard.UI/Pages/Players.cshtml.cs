using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Players(PlayerHttpClient playerHttpClient, StampCardHttpClient stampCardHttpClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty]
    public Player NewPlayer { get; set; }

    public List<Player> Items { get; set; } = [];
    
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadItemsAsync();
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToPage("/Index");
        }
    }

    private async Task LoadItemsAsync()
    {
        var response = await playerHttpClient.ListAsync(Team);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message ?? "");
            return;
        }

        // the delete dialog names the number of stamp cards removed along with the player
        var stampCardResponse = await stampCardHttpClient.ListAsync(Team);
        if (stampCardResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, stampCardResponse.Message ?? "");
            return;
        }

        var stampCardCounts = stampCardResponse.Data
            .GroupBy(card => card.PlayerId)
            .ToDictionary(group => group.Key, group => group.Count());
        Items = response.Data.Select(dto => new Player
        {
            Id = dto.Id, 
            FirstName = dto.FirstName, 
            LastName = dto.LastName, 
            Birthdate = dto.Birthdate, 
            Active = dto.Active,
            StampCardCount = stampCardCounts.GetValueOrDefault(dto.Id)
        }).ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var response = await playerHttpClient.CreateAsync(Team, NewPlayer.FirstName, NewPlayer.LastName, NewPlayer.Birthdate);
        if (!response.HasError) return RedirectToPage();
        
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
    
    /// <summary>
    /// Alternative to deleting, the player and its stamp cards are kept.
    /// </summary>
    public async Task<IActionResult> OnGetDisableAsync(Guid id)
    {
        var response = await playerHttpClient.SetActiveAsync(Team, id, false);
        if (!response.HasError) return RedirectToPage();
        
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
    
    public async Task<IActionResult> OnGetDeleteAsync(Guid id)
    {
        var response = await playerHttpClient.DeleteAsync(Team, id);
        if (!response.HasError) return RedirectToPage();
        
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
}

public class Player
{
    public Guid? Id { get; set; }
        
    [Required(ErrorMessage = "Vorname darf nicht leer sein!")]
    [MinLength(2, ErrorMessage = "Vorname muss mindestens 2 Zeichen lang sein!")]
    public string FirstName { get; set; }
        
    [Required(ErrorMessage = "Nachname darf nicht leer sein!")]
    [MinLength(2, ErrorMessage = "Nachname muss mindestens 2 Zeichen lang sein!")]
    public string LastName { get; set; }
        
    [Required(ErrorMessage = "Geburstdatum darf nicht leer sein!")]
    [DataType(DataType.Date, ErrorMessage = "Ungültiges Datum!")]
    public DateOnly Birthdate { get; set; }
        
    public bool Active { get; set; }

    /// <summary>
    /// The active state when the player was loaded, so that it is only changed if the user changed it.
    /// </summary>
    public bool WasActive { get; set; }
    
    public long ConcurrencyToken { get; set; }

    /// <summary>
    /// The number of stamp cards of the player, they are deleted along with it.
    /// </summary>
    public int StampCardCount { get; set; }
}