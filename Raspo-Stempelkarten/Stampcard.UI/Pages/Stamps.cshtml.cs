using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class Stamps : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }
    
    public void OnGet()
    {
        
    }
}