using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class StampCards : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    public List<StampCard> Items { get; set; } = [];
    
    [BindProperty]
    public string PlayerName { get; set; }

    [BindProperty]
    public int AccountingYear { get; set; }

    public void OnGet()
    {
        
    }
}

public class StampCard
{
    public string Id { get; set; }
    
    public Guid PlayerId { get; set; }
    
    public string PlayerName { get; set; }
    
    public int AccountingYear { get; set; }
    
}