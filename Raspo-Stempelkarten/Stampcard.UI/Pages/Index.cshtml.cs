using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class Index : PageModel
{
    public required List<string> Items { get; set; } = [];
    [BindProperty] public int BirthYear { get; set; } = DateTime.UtcNow.Year;

    public Task OnGetAsync()
    {
        Items.Add("raspo1926-2014");
        Items.Add("raspo1926-2015");
        Items.Add("raspo1926-2016");
        Items.Add("raspo1926-2017");
        Items.Add("raspo1926-2018");
        return Task.CompletedTask;
    }

    public IActionResult OnGetDelete(string id)
    {
        return RedirectToPage("/Team");
    }
    
    public IActionResult OnGetItem(string target, string id)
    {
        return RedirectToPage(target, new { Team = id });
    }
}