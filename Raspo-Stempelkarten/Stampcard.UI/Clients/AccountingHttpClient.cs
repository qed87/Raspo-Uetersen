using RestSharp;

namespace Stampcard.UI.Clients;

/// <summary>
/// Rest client for the accounting, which evaluates the stamp cards of all teams.
/// Only users with the accounting role may use it.
/// </summary>
/// <param name="restClient">The underlying rest client.</param>
public class AccountingHttpClient(IRestClient restClient)
{
    /// <summary>
    /// Gets the players of all teams with fewer accepted stamps than <param name="requiredStamps">required</param>
    /// in the <param name="accountingYear">accounting year</param>, including the active players without a stamp card for it.
    /// </summary>
    /// <param name="accountingYear">The accounting year.</param>
    /// <param name="requiredStamps">The number of accepted stamps required in the accounting year.</param>
    public Task<ResponseWrapperDto<List<IncompleteStampCardReadDto>>> ListIncompleteStampCardsAsync(int accountingYear, int requiredStamps)
    {
        var request = new RestRequest("/api/accounting/stampcard/incomplete")
            .AddQueryParameter("accountingYear", accountingYear)
            .AddQueryParameter("requiredStamps", requiredStamps);
        return restClient.SendAsync<List<IncompleteStampCardReadDto>>(request);
    }
}
