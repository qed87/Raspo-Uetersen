using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Core;

/// <summary>
/// Tracks all published events.
/// </summary>
public interface IEventDataChangeTracker
{
    /// <summary>
    /// Returns the tracked events (changes).
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<EventData> GetChanges();
}