using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampAdded : INotification
{
    public Guid Id { get; set; }
    
    public Guid StampCardId { get; set; }
    public string Reason { get; set; }
    public string IssuedBy { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
}