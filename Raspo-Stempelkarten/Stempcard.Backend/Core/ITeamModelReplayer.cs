using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Core;

/// <summary>
/// 
/// </summary>
public interface ITeamModelReplayer
{
    /// <summary>
    /// The stream id.
    /// </summary>
    string StreamId { get; }
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
    Team GetModel();
}