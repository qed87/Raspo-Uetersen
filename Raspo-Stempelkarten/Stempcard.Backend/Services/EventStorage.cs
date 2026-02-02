using JetBrains.Annotations;
using KurrentDB.Client;
using Polly;

namespace Raspo_Stempelkarten_Backend.Services;

/// <inheritdoc />
[UsedImplicitly]
public class EventStorage(
    KurrentDBClient kurrentDbClient) 
    : IEventStorage
{
    /// <summary>
    /// Store user events in the database.
    /// </summary>
    /// <param name="streamId">The stream id.</param>
    /// <param name="concurrencyToken">The concurrency token.</param>
    /// <param name="events">The events to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result object with updated concurrency token.</returns>
    public async Task<ulong> StoreAsync(
        string streamId,
        ulong? concurrencyToken,
        IEnumerable<EventData> events,
        CancellationToken cancellationToken = default)
    {
        var writeResult = await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(250 * i))
            .ExecuteAsync(async () => await kurrentDbClient.AppendToStreamAsync(
                streamId,
                !concurrencyToken.HasValue
                    ? StreamState.NoStream
                    : StreamState.StreamRevision(concurrencyToken!.Value),
                events,
                cancellationToken: cancellationToken));
        return (ulong) writeResult.NextExpectedStreamState.ToInt64();
    }
}