using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IEventDataChangeTracker
{
    public IReadOnlyList<EventData> GetChanges();
}