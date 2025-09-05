using KurrentDB.Client;
using LiteBus.Events.Abstractions;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

internal class StempelkartenModelLoader(
    IEventMediator mediator,
    KurrentDBClient kurrentDbClient, 
    IStreamNameProvider streamNameProvider) : IStempelkartenModelLoader
{
    /// <summary>
    /// Load <see cref="StampCardAggregate"/> from storage.
    /// </summary>
    /// <param name="team">The team name.</param>
    /// <param name="season">The season.</param>
    /// <returns>The loaded <see cref="StampCardAggregate" />.</returns>
    public async Task<IStampCardAggregate> LoadModelAsync(
        string team, string season)
    {
        var result = kurrentDbClient.ReadStreamAsync(
            Direction.Forwards,
            streamNameProvider.GetStreamName(team, season),
            StreamPosition.Start);
        
        var replayer = new StempelkartenReplayer(mediator, team, season);
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
        var decoratedAggregate =  new StampCardAggregateEventDetectorDecorator(modelAggregate, mediator);
        return decoratedAggregate;
    }
}