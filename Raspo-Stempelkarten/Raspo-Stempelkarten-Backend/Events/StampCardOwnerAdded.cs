using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardOwnerAdded : INotification
{
    public Guid StampCardId { get; set; }
    
    public string Name { get; set; } = null!;
    
}