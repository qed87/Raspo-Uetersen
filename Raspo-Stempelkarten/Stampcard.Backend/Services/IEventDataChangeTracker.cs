using KurrentDB.Client;

namespace StampCard.Backend.Services;

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