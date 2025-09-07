using FluentResults;
using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStampCardModelStorage
{
    /// <summary>
    /// Store user events in the database.
    /// </summary>
    /// <param name="team">The team name.</param>
    /// <param name="season">The season.</param>
    /// <param name="concurrencyToken">The concurrency token.</param>
    /// <param name="events">The events to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result object with updated concurrency token.</returns>
    public Task<Result<long>> StoreAsync(string team, string season, ulong? concurrencyToken, 
        IEnumerable<EventData> events, CancellationToken cancellationToken = default);
}