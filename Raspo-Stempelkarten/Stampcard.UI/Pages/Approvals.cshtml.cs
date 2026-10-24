using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

/// <summary>
/// Stamps submitted by external users that wait for the approval of a coach.
/// </summary>
public class Approvals(StampCardHttpClient stampCardHttpClient, PlayerHttpClient playerHttpClient, TeamHttpClient teamHttpClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }

    /// <summary>
    /// The stamps selected for approving or rejecting them together.
    /// </summary>
    [BindProperty]
    public List<Guid> SelectedStampIds { get; set; } = [];

    public List<PendingStamp> Items { get; set; } = [];

    public string TeamName { get; set; } = "";

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadItemsAsync();
        return Page();
    }

    private async Task<ResponseWrapperDto<List<PendingStampReadDto>>> LoadItemsAsync()
    {
        var pendingResponse = await stampCardHttpClient.ListPendingStampsAsync(Team);
        if (pendingResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, pendingResponse.Message!);
            return pendingResponse;
        }

        var playerResponse = await playerHttpClient.ListAsync(Team);
        if (playerResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, playerResponse.Message!);
            return pendingResponse;
        }

        var stampCardResponse = await stampCardHttpClient.ListAsync(Team);
        if (stampCardResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, stampCardResponse.Message!);
            return pendingResponse;
        }

        var teamResponse = await teamHttpClient.GetTeamAsync(Team);
        if (teamResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, teamResponse.Message!);
            return pendingResponse;
        }

        TeamName = teamResponse.Data.Name;

        var playerNames = playerResponse.Data.ToDictionary(player => player.Id, player => $"{player.LastName}, {player.FirstName}");
        var accountingYears = stampCardResponse.Data.ToDictionary(card => card.Id, card => card.AccountingYear);
        Items = pendingResponse.Data
            .OrderBy(stamp => stamp.IssuedOn)
            .Select(stamp => new PendingStamp
            {
                Id = stamp.Id,
                StampCardId = stamp.StampCardId,
                PlayerName = playerNames.GetValueOrDefault(stamp.PlayerId, "(Spieler gelöscht)"),
                AccountingYear = accountingYears.GetValueOrDefault(stamp.StampCardId),
                Reason = stamp.Reason,
                Issuer = stamp.Issuer,
                IssuedOn = stamp.IssuedOn
            })
            .ToList();
        return pendingResponse;
    }

    public Task<IActionResult> OnPostApproveAsync(Guid stampId)
    {
        return ApproveAsync([stampId]);
    }

    public Task<IActionResult> OnPostApproveSelectedAsync()
    {
        return ApproveAsync(SelectedStampIds);
    }

    public Task<IActionResult> OnPostRejectAsync(Guid stampId)
    {
        return RejectAsync([stampId]);
    }

    public Task<IActionResult> OnPostRejectSelectedAsync()
    {
        return RejectAsync(SelectedStampIds);
    }

    private async Task<IActionResult> ApproveAsync(List<Guid> stampIds)
    {
        if (stampIds.Count == 0) return await NothingSelectedAsync();

        var response = await stampCardHttpClient.ApproveStampsAsync(Team, stampIds);
        if (!response.HasError) return RedirectToPage();
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }

    // the backend rejects one stamp at a time and needs its stamp card, which is taken from the current pending stamps
    private async Task<IActionResult> RejectAsync(List<Guid> stampIds)
    {
        if (stampIds.Count == 0) return await NothingSelectedAsync();

        var pendingResponse = await stampCardHttpClient.ListPendingStampsAsync(Team);
        if (pendingResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, pendingResponse.Message!);
            await LoadItemsAsync();
            return Page();
        }

        var stampCardIds = pendingResponse.Data.ToDictionary(stamp => stamp.Id, stamp => stamp.StampCardId);
        foreach (var stampId in stampIds)
        {
            if (!stampCardIds.TryGetValue(stampId, out var stampCardId))
            {
                ModelState.AddModelError(string.Empty,
                    "Ein ausgewählter Stempel wartet nicht mehr auf Freigabe. Die Liste wurde neu geladen.");
                break;
            }

            var response = await stampCardHttpClient.RejectStampAsync(Team, stampCardId, stampId);
            if (response.HasError)
            {
                // stamps rejected before stay rejected, the reloaded list shows what is left
                ModelState.AddModelError(string.Empty, response.Message!);
                break;
            }
        }

        if (ModelState.IsValid) return RedirectToPage();
        await LoadItemsAsync();
        return Page();
    }

    private async Task<IActionResult> NothingSelectedAsync()
    {
        ModelState.AddModelError(string.Empty, "Bitte wählen Sie mindestens einen Stempel aus.");
        await LoadItemsAsync();
        return Page();
    }
}

public class PendingStamp
{
    public Guid Id { get; set; }

    public Guid StampCardId { get; set; }

    public string PlayerName { get; set; } = "";

    public int AccountingYear { get; set; }

    public string Reason { get; set; } = "";

    /// <summary>
    /// The name the external user entered when submitting the stamp.
    /// </summary>
    public string Issuer { get; set; } = "";

    public DateTimeOffset IssuedOn { get; set; }
}
