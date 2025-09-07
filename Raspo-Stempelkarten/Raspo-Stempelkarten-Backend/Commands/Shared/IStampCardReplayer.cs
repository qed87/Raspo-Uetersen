using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStampCardReplayer
{
    /// <summary>
    /// Replays a set of resolved events.
    /// </summary>
    void Replay(ResolvedEvent[] resolvedEvents);
    
    /// <summary>
    /// Replays a single resolved events.
    /// </summary>
    void Replay(ResolvedEvent resolvedEvent);

    /// <summary>
    /// Get the replayed aggregate model.
    /// </summary>
    /// <returns>The </returns>
    StampCardAggregate GetModel();
}