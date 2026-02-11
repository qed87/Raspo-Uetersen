using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Players(PlayerHttpClient playerHttpClient) : PageModel
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
        if (!response.HasError)
        {
            Items = response.Data.Select(dto => new Player
            {
                Id = dto.Id, 
                FirstName = dto.FirstName, 
                LastName = dto.LastName, 
                Birthdate = dto.Birthdate, 
                Birthplace = dto.Birthplace,
                Active = dto.Active
            }).ToList();
            return;
        }
        
        ModelState.AddModelError(string.Empty, response.Message ?? "");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var response = await playerHttpClient.CreateAsync(Team, NewPlayer.FirstName, NewPlayer.LastName, NewPlayer.Birthdate, NewPlayer.Birthplace);
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
    public string FirstName { get; set; }
        
    [Required(ErrorMessage = "Nachname darf nicht leer sein!")]
    public string LastName { get; set; }
        
    [Required(ErrorMessage = "Geburstdatum darf nicht leer sein!")]
    [DataType(DataType.Date, ErrorMessage = "Ungültiges Datum!")]
    public DateOnly Birthdate { get; set; }
        
    [Required(ErrorMessage = "Geburtsort darf nicht leer sein!")]
    [MinLength(2, ErrorMessage = "Geburtsort muss mehr länger als 1 Zeichen sein!")]
    public string Birthplace { get; set; }
    
    public bool Active { get; set; }
    
    public ulong ConcurrencyToken { get; set; }
}