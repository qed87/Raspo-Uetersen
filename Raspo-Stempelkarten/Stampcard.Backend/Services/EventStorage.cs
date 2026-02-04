using JetBrains.Annotations;
using KurrentDB.Client;
using Polly;

namespace StampCard.Backend.Services;

/// <inheritdoc />
[UsedImplicitly]
public class EventStorage(
    KurrentDBClient kurrentDbClient, 
    ILogger<EventStorage> logger) 
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
        logger.LogInformation("Storing events to repository '{StreamId}' and revision '{Revision}'.", streamId, concurrencyToken);
        var writeResult = await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(250 * i))
            .ExecuteAsync(async () =>
            {
                logger.LogTrace("(Re-)try write to repository '{StreamId}'.", streamId);
                return await kurrentDbClient.AppendToStreamAsync(
                    streamId,
                    !concurrencyToken.HasValue
                        ? StreamState.NoStream
                        : StreamState.StreamRevision(concurrencyToken!.Value),
                    events,
                    cancellationToken: cancellationToken);
            });
        return (ulong) writeResult.NextExpectedStreamState.ToInt64();
    }
}