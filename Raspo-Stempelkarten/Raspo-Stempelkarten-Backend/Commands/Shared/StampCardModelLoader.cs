using DispatchR;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

internal class StampCardModelLoader(
    IMediator mediator,
    KurrentDBClient kurrentDbClient, 
    IStreamNameProvider streamNameProvider) : IStampCardModelLoader
{
    /// <summary>
    /// Load <see cref="StampCardAggregate"/> from storage.
    /// </summary>
    /// <param name="season">The season.</param>
    /// <param name="team">The team name.</param>
    /// <returns>The loaded <see cref="StampCardAggregate" />.</returns>
    public async Task<IStampCardAggregate> LoadModelAsync(string season, string team)
    {
        var result = kurrentDbClient.ReadStreamAsync(
            Direction.Forwards,
            streamNameProvider.GetStreamName(season, team),
            StreamPosition.Start);
        
        var replayer = new StampCardReplayer(season, team);
        ulong? streamRevision = null;
        if (await result.ReadState == ReadState.Ok)
        {
            await foreach (var resolvedEvent in result)
            {
                replayer.Replay(resolvedEvent);
                streamRevision = resolvedEvent.OriginalEventNumber.ToUInt64();
            }
        }

        var modelAggregate = replayer.GetModel();
        modelAggregate.ConcurrencyToken = streamRevision;
        var decoratedAggregate =  new StampCardAggregateEventDetectorDecorator(season, team, modelAggregate, mediator);
        return decoratedAggregate;
    }
}