using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class Team : PageModel
{
    [BindProperty(Name = "team", SupportsGet = true)]
    public string Id { get; set; }

    public List<string> Items { get; set; } = [];
    
    [BindProperty]
    [Required]
    [EmailAddress]
    public string Name { get; set; }

    public void OnGet()
    {
        
    }

    
}