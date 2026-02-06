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
    
    public async Task OnGetAsync()
    {
        await LoadItemsAsync();
    }

    private async Task LoadItemsAsync()
    {
        var response = await playerHttpClient.ListAsync(Team);
        Items = response.Data.Select(dto => new Player
        {
            Id = dto.Id, 
            FirstName = dto.FirstName, 
            LastName = dto.LastName, 
            Birthdate = dto.Birthdate, 
            Birthplace = dto.Birthplace
        }).ToList();
    }

    public IActionResult OnPost()
    {
        if (ModelState.IsValid)
        {
            // Member Speichern
            return RedirectToPage();
        }
        
        return Page();
    }
    
    public IActionResult OnGetDelete(Guid id)
    {
        return RedirectToPage();
    }
    
    public class Player
    {
        public Guid? Id { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateOnly Birthdate { get; set; }
        
        [Required]
        [MinLength(2)]
        public string Birthplace { get; set; }
    }
}