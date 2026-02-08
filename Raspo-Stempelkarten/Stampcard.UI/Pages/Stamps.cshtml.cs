using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Stamps(StampCardHttpClient stampCardHttpClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid StampCardId { get; set; }
    
    public List<Stamp> Items { get; set; } = [];
    
    [BindProperty] 
    public UpdateStamp Fields
    {
        get;
        set;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadItemsAsync();
        return Page();
    }

    private async Task LoadItemsAsync()
    {
        var response = await stampCardHttpClient.ListStampsAsync(Team, StampCardId);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message!);
            return;
        }

        foreach (var stampReadDto in response.Data)
        {
            Items.Add(new Stamp
            {
                Id = stampReadDto.Id,
                Issuer =  stampReadDto.Issuer,
                IssuedOn = stampReadDto.IssuedOn,
                Reason = stampReadDto.Reason
            });
        }
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var response = await stampCardHttpClient.StampAsync(Team, StampCardId, Fields.Reason);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetDeleteAsync(Guid id)
    {
        var response = await stampCardHttpClient.DeleteStampAsync(Team, StampCardId, id);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
}

public class UpdateStamp
{
    public string Reason { get; set; }
}

public class Stamp
{
    public Guid Id { get; set; }
    
    public string Reason { get; set; }
    
    public string Issuer { get; set; }
    
    public DateTimeOffset IssuedOn { get; set; }
    
}