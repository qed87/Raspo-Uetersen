using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class Stamps : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string StampCardId { get; set; }
    
    public List<Stamp> Items { get; set; } = [];
    
    public UpdateStamp Fields = new();

    public void OnGet()
    {
        
    }
}

[BindProperties] 
public class UpdateStamp
{
    [BindProperty]
    public string Reason { get; set; }
}

public class Stamp
{
    public Guid Id { get; set; }
    
    public string Reason { get; set; }
    
    public string Issuer { get; set; }
    
    public DateTimeOffset IssuedOn { get; set; }
    
}