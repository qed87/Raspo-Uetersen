using KurrentDB.Client;

namespace StampCard.Backend.Services.Interfaces;

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