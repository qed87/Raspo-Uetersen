using KurrentDB.Client;
using StampCard.Backend.Model;

namespace StampCard.Backend.Services;

/// <summary>
/// 
/// </summary>
public interface ITeamModelReplayer
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
    Team GetModel();
}