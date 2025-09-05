using FluentResults;
using JetBrains.Annotations;
using KurrentDB.Client;
using Polly;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

[UsedImplicitly]
public class StempelkartenModelStorage(KurrentDBClient kurrentDbClient, IStreamNameProvider streamNameProvider) 
    : IStempelkartenModelStorage
{
    /// <summary>
    /// Store user events in the database.
    /// </summary>
    /// <param name="team">The team name.</param>
    /// <param name="season">The season.</param>
    /// <param name="concurrencyToken">The concurrency token.</param>
    /// <param name="eventData">The events to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result object with updated concurrency token.</returns>
    public async Task<Result<long>> StoreAsync(string team, string season, ulong? concurrencyToken, 
        IEnumerable<EventData> eventData, CancellationToken cancellationToken = default)
    {
        IWriteResult writeResult;
        try
        {
            writeResult = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(250 * i))
                .ExecuteAsync(async () => await kurrentDbClient.AppendToStreamAsync(
                    streamNameProvider.GetStreamName(team, season),
                    !concurrencyToken.HasValue
                        ? StreamState.NoStream
                        : StreamState.StreamRevision(concurrencyToken!.Value),
                    eventData,
                    cancellationToken: cancellationToken));
        }
        catch (Exception)
        {
            return Result.Fail("Stempelkarte konnte nicht erstellt werden.");
        }

        return Result.Ok(writeResult.NextExpectedStreamState.ToInt64());
    }
}