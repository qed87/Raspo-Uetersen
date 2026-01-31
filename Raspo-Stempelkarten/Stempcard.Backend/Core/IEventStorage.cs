using FluentResults;
using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Core;

/// <summary>
/// Storage for events.
/// </summary>
public interface IEventStorage
{
    /// <summary>
    /// Store user events in the database.
    /// </summary>
    /// <param name="streamId">The team name.</param>
    /// <param name="concurrencyToken">The concurrency token.</param>
    /// <param name="events">The events to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result object with updated concurrency token.</returns>
    public Task<Result<long>> StoreAsync(
        string streamId,
        ulong? concurrencyToken,
        IEnumerable<EventData> events,
        CancellationToken cancellationToken = default);
}