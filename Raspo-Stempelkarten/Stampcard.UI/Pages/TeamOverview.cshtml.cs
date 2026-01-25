using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class TeamOverview : PageModel
{
    public required List<string> Teams { get; set; } = [];
    [BindProperty] public int BirthYear { get; set; } = DateTime.UtcNow.Year;

    public Task OnGetAsync()
    {
        Teams.Add("raspo1926-2014");
        Teams.Add("raspo1926-2015");
        Teams.Add("raspo1926-2016");
        Teams.Add("raspo1926-2017");
        Teams.Add("raspo1926-2018");
        return Task.CompletedTask;
    }

    public IActionResult OnGetDeleteAsync(string id)
    {
        return RedirectToPage("/TeamOverview");
    }
}