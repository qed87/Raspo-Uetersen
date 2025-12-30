using KurrentDB.Client;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStampCardChangeTracker
{
    IEnumerable<EventData> GetChanges();
}