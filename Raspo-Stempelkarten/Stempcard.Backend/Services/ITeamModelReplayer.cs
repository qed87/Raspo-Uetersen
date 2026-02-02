using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Services;

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