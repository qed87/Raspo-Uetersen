using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class StampCards : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    public void OnGet()
    {
        
    }
}