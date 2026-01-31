using DispatchR;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Exceptions;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Core;

/// <summary>
/// The team model loader.
/// </summary>
/// <param name="mediator">The mediator.</param>
/// <param name="kurrentDbClient">The kurrent db client.</param>
[UsedImplicitly]
public class TeamModelLoader(
    IMediator mediator,
    KurrentDBClient kurrentDbClient)
    : ITeamModelLoader
{
    /// <inheritdoc />
    public async Task<ITeamAggregate> LoadModelAsync(string streamId, long? expectedVersion = null, long? position = null)
    {
        var startFromPosition = position is not null 
            ? StreamPosition.FromInt64(position.Value) 
            : StreamPosition.Start; 
        var result = kurrentDbClient.ReadStreamAsync(
            Direction.Forwards,
            streamId,
            startFromPosition);
        var replayer = new TeamModelReplayer(mediator, streamId);
        StreamPosition? streamRevision = null;
        if (await result.ReadState == ReadState.Ok)
        {
            await foreach (var resolvedEvent in result)
            {
                replayer.Replay(resolvedEvent);
                streamRevision = resolvedEvent.OriginalEventNumber;
            }
        }

        if (expectedVersion is not null && (streamRevision is null || streamRevision.GetValueOrDefault().ToInt64() != expectedVersion))
        {
            throw new ModelConcurrencyException();
        }

        var modelAggregate = replayer.GetModel();
        modelAggregate.Id = streamId;
        modelAggregate.Version = streamRevision?.ToUInt64();
        return modelAggregate;
    }
}