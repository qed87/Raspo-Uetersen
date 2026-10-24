using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Stamps(StampCardHttpClient stampCardHttpClient, TeamHttpClient teamHttpClient) : PageModel
{
    private const string CreatedSharedCodeKey = "CreatedSharedCode";

    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid StampCardId { get; set; }
    
    public List<Stamp> Items { get; set; } = [];

    public List<SharedCodeReadDto> SharedCodes { get; set; } = [];

    /// <summary>
    /// Only coaches of the team may share its stamp cards.
    /// </summary>
    public bool IsCoach { get; set; }

    /// <summary>
    /// The code created by the previous request, its link is shown in a dialog.
    /// </summary>
    public string? CreatedSharedCode { get; set; }

    [BindProperty] 
    public UpdateStamp Fields
    {
        get;
        set;
    }

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
        var response = await stampCardHttpClient.GetAsync(Team, StampCardId);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message!);
            return;
        }

        foreach (var stampReadDto in response.Data.Stamps)
        {
            Items.Add(new Stamp
            {
                Id = stampReadDto.Id,
                Issuer =  stampReadDto.Issuer,
                IssuedOn = stampReadDto.IssuedOn,
                Reason = stampReadDto.Reason,
                Pending = stampReadDto.Status == "pending"
            });
        }

        var teamResponse = await teamHttpClient.GetTeamAsync(Team);
        if (teamResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, teamResponse.Message!);
            return;
        }

        IsCoach = teamResponse.Data.Coaches.Contains(User.Identity?.Name ?? string.Empty);
        if (!IsCoach) return;

        var sharedCodeResponse = await stampCardHttpClient.ListSharedCodesAsync(Team, StampCardId);
        if (sharedCodeResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, sharedCodeResponse.Message!);
            return;
        }

        SharedCodes.AddRange(sharedCodeResponse.Data);
        CreatedSharedCode = TempData[CreatedSharedCodeKey] as string;
    }

    /// <summary>
    /// The public link an external user opens to see the stamp card.
    /// </summary>
    public string SharedLinkOf(string code)
    {
        return Url.Page("/SharedStampCard", null, new { team = Team, stampCardId = StampCardId, code }, Request.Scheme)!;
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

    public async Task<IActionResult> OnPostShareAsync()
    {
        ModelState.Remove($"{nameof(Fields)}.{nameof(UpdateStamp.Reason)}");
        var response = await stampCardHttpClient.ShareAsync(Team, StampCardId);
        if (!response.HasError)
        {
            // survives the redirect, so the dialog with the new link opens on the reloaded page
            TempData[CreatedSharedCodeKey] = response.Data.Code;
            return RedirectToPage();
        }
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnGetDeleteSharedCodeAsync(Guid id)
    {
        var response = await stampCardHttpClient.DeleteSharedCodeAsync(Team, StampCardId, id);
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
    
    /// <summary>
    /// Stamps submitted by external users are pending until a coach approves them.
    /// </summary>
    public bool Pending { get; set; }
}