using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardOwnerRemoved : INotification
{
    public Guid StampCardId { get; set; }
    
    public string Name { get; set; } = null!;
}