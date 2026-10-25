using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class Stamps(StampCardHttpClient stampCardHttpClient, TeamHttpClient teamHttpClient, PlayerHttpClient playerHttpClient) : PageModel
{
    private const string CreatedSharedCodeKey = "CreatedSharedCode";

    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid StampCardId { get; set; }
    
    public List<Stamp> Items { get; set; } = [];

    public string PlayerName { get; set; } = "";

    public int? AccountingYear { get; set; }

    public List<SharedCodeReadDto> SharedCodes { get; set; } = [];

    /// <summary>
    /// Only coaches of the team may share its stamp cards.
    /// </summary>
    public bool IsCoach { get; set; }

    /// <summary>
    /// The code created by the previous request, its link is shown in a dialog.
    /// </summary>
    public SharedCodeReadDto? CreatedSharedCode { get; set; }

    /// <summary>
    /// Holder of a new share code. Bound by the share handler only, so that the other forms are not validated against it.
    /// </summary>
    public NewSharedCode Share { get; set; } = new();

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
        var response = await stampCardHttpClient.GetAsync(Team, StampCardId);
        if (response.HasError)
        {
            ModelState.AddModelError(string.Empty, response.Message!);
            return;
        }

        AccountingYear = response.Data.AccountingYear;
        var playerResponse = await playerHttpClient.GetAsync(Team, response.Data.PlayerId);
        PlayerName = playerResponse.HasError
            ? "(Spieler gelöscht)"
            : $"{playerResponse.Data.FirstName} {playerResponse.Data.LastName}";

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

        IsCoach = User.IsCoachOf(teamResponse.Data);
        if (!IsCoach) return;

        var sharedCodeResponse = await stampCardHttpClient.ListSharedCodesAsync(Team, StampCardId);
        if (sharedCodeResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, sharedCodeResponse.Message!);
            return;
        }

        SharedCodes.AddRange(sharedCodeResponse.Data);
        var createdCode = TempData[CreatedSharedCodeKey] as string;
        CreatedSharedCode = SharedCodes.FirstOrDefault(sharedCode => sharedCode.Code == createdCode);
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

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        // the reason of a new stamp is not part of this form
        ModelState.Clear();
        var response = await stampCardHttpClient.DeleteStampAsync(Team, StampCardId, id);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostShareAsync(NewSharedCode share)
    {
        ModelState.Clear();
        Share = share;
        if (!TryValidateModel(share, nameof(Share)))
        {
            await LoadItemsAsync();
            return Page();
        }

        var response = await stampCardHttpClient.ShareAsync(Team, StampCardId, share.HolderName);
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

    /// <summary>
    /// The link keeps showing the stamp card, but no more stamps can be submitted with it.
    /// </summary>
    public async Task<IActionResult> OnPostRevokeSharedCodeAsync(Guid id)
    {
        ModelState.Clear();
        var response = await stampCardHttpClient.RevokeSharedCodeAsync(Team, StampCardId, id);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteSharedCodeAsync(Guid id)
    {
        ModelState.Clear();
        var response = await stampCardHttpClient.DeleteSharedCodeAsync(Team, StampCardId, id);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
}

public class NewSharedCode
{
    [Required(ErrorMessage = "Name darf nicht leer sein!")]
    [MinLength(2, ErrorMessage = "Name muss mindestens 2 Zeichen lang sein!")]
    public string HolderName { get; set; } = "";
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