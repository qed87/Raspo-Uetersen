using KurrentDB.Client;

namespace StampCard.Backend.Services.Interfaces;

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
    /// <returns>The updated concurrency token.</returns>
    public Task<ulong> StoreAsync(
        string streamId,
        ulong? concurrencyToken,
        IEnumerable<EventData> events,
        CancellationToken cancellationToken = default);
}