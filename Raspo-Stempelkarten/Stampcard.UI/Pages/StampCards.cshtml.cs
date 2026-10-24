using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stampcard.UI.Clients;

namespace Stampcard.UI.Pages;

public class StampCards(StampCardHttpClient stampCardHttpClient, PlayerHttpClient playerHttpClient, TeamHttpClient teamHttpClient) : PageModel
{
    private const string AllYears = "alle";
    private const string StampedKey = "Stamped";

    [BindProperty(SupportsGet = true)]
    public string Team { get; set; }

    /// <summary>
    /// The accounting year the list is filtered by, <c>alle</c> for all years. Without filter the current year is shown.
    /// </summary>
    [BindProperty(Name = "year", SupportsGet = true)]
    public string? Year { get; set; }

    public string TeamName { get; set; } = "";

    public List<SelectListItem> Players { get; set; } = [];

    /// <summary>
    /// The accounting years of all stamp cards and the current year, newest first.
    /// </summary>
    public List<int> AvailableYears { get; set; } = [];

    public List<StampCard> Items { get; set; } = [];

    [BindProperty]
    public string PlayerName { get; set; }

    [BindProperty]
    public int AccountingYear { get; set; } = DateTime.UtcNow.Year;

    /// <summary>
    /// The stamp cards selected for stamping them at once, e.g. after a training.
    /// </summary>
    [BindProperty]
    public List<Guid> SelectedStampCardIds { get; set; } = [];

    [BindProperty]
    public string? QuickStampReason { get; set; }

    /// <summary>
    /// The number of stamps given by the previous request, a confirmation is shown.
    /// </summary>
    public int? StampedCount { get; set; }

    public int? SelectedYear => Year == AllYears
        ? null
        : int.TryParse(Year, out var year) ? year : DateTime.UtcNow.Year;

    public async Task<IActionResult> OnGetAsync()
    {
        StampedCount = TempData[StampedKey] as int?;
        await LoadItemsAsync();
        return Page();
    }

    private async Task LoadItemsAsync()
    {
        var teamResponse = await teamHttpClient.GetTeamAsync(Team);
        var playerResponse = await playerHttpClient.ListAsync(Team);
        var stampCardResponse = await stampCardHttpClient.ListAsync(Team);
        bool hasErrors = false;
        if (teamResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, teamResponse.Message!);
            hasErrors = true;
        }

        if (playerResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, playerResponse.Message!);
            hasErrors = true;
        }

        if (stampCardResponse.HasError)
        {
            ModelState.AddModelError(string.Empty, stampCardResponse.Message!);
            hasErrors = true;
        }

        if (hasErrors) return;

        TeamName = teamResponse.Data.Name;
        foreach (var playerReadDto in playerResponse.Data.OrderBy(player => player.LastName).ThenBy(player => player.FirstName))
            Players.Add(new SelectListItem($"{playerReadDto.LastName}, {playerReadDto.FirstName}", Convert.ToString(playerReadDto.Id)));

        AvailableYears = stampCardResponse.Data
            .Select(card => card.AccountingYear)
            .Append(DateTime.UtcNow.Year)
            .Distinct()
            .OrderDescending()
            .ToList();

        var playersDict =  playerResponse.Data.ToDictionary(x => x.Id, x => $"{x.LastName}, {x.FirstName}");
        foreach (var stampCardReadDto in stampCardResponse.Data.Where(card => SelectedYear is null || card.AccountingYear == SelectedYear))
        {
            Items.Add(new StampCard
            {
                Id = stampCardReadDto.Id,
                PlayerId = stampCardReadDto.PlayerId,
                PlayerName = playersDict.GetValueOrDefault(stampCardReadDto.PlayerId, "(Spieler gelöscht)"),
                AccountingYear = stampCardReadDto.AccountingYear
            });
        }

        Items = Items.OrderByDescending(card => card.AccountingYear).ThenBy(card => card.PlayerName).ToList();
    }

    public async Task<IActionResult> OnPostManualAsync(Guid playerName,  int accountingYear)
    {
        var response = await stampCardHttpClient.CreateAsync(Team, playerName, accountingYear);
        if (!response.HasError) return RedirectToPage(new { year = accountingYear });
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }

    /// <summary>
    /// Creates stamp cards of the accounting year for all active players that do not have one yet.
    /// </summary>
    public async Task<IActionResult> OnPostAutoAsync(int accountingYear)
    {
        var response = await CreateMissingStampCardsAsync(accountingYear);
        if (!response.HasError) return RedirectToPage(new { year = accountingYear });
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }

    private async Task<ResponseWrapperDto> CreateMissingStampCardsAsync(int accountingYear)
    {
        var playerResponse = await playerHttpClient.ListAsync(Team);
        if (playerResponse.HasError) return playerResponse;
        var stampCardResponse = await stampCardHttpClient.ListAsync(Team, accountingYear);
        if (stampCardResponse.HasError) return stampCardResponse;

        var playersWithStampCard = stampCardResponse.Data.Select(card => card.PlayerId).ToHashSet();
        foreach (var player in playerResponse.Data.Where(player => player.Active && !playersWithStampCard.Contains(player.Id)))
        {
            var response = await stampCardHttpClient.CreateAsync(Team, player.Id, accountingYear);
            if (response.HasError) return response;
        }

        return new ResponseWrapperDto();
    }

    /// <summary>
    /// Gives the selected stamp cards a stamp with the same reason. The backend stamps one card at a time,
    /// so cards stamped before an error keep their stamp.
    /// </summary>
    public async Task<IActionResult> OnPostStampSelectedAsync()
    {
        // the fields for creating stamp cards are not part of this form
        ModelState.Clear();
        if (SelectedStampCardIds.Count == 0)
            ModelState.AddModelError(string.Empty, "Bitte wählen Sie mindestens eine Stempelkarte aus.");
        if (string.IsNullOrWhiteSpace(QuickStampReason))
            ModelState.AddModelError(nameof(QuickStampReason), "Grund darf nicht leer sein!");
        if (!ModelState.IsValid)
        {
            await LoadItemsAsync();
            return Page();
        }

        var stamped = 0;
        foreach (var stampCardId in SelectedStampCardIds.Distinct())
        {
            var response = await stampCardHttpClient.StampAsync(Team, stampCardId, QuickStampReason!);
            if (response.HasError)
            {
                ModelState.AddModelError(string.Empty,
                    $"{response.Message} {stamped} von {SelectedStampCardIds.Count} Stempelkarten wurden bereits gestempelt.");
                await LoadItemsAsync();
                return Page();
            }

            stamped++;
        }

        TempData[StampedKey] = stamped;
        return RedirectToPage(new { year = Year });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        ModelState.Clear();
        var response = await stampCardHttpClient.DeleteAsync(Team, id);
        if (!response.HasError) return RedirectToPage(new { year = Year });
        ModelState.AddModelError(string.Empty, response.Message!);
        await LoadItemsAsync();
        return Page();
    }
}

public class StampCard
{
    public Guid Id { get; set; }

    public Guid PlayerId { get; set; }

    public string PlayerName { get; set; }

    public int AccountingYear { get; set; }
}
