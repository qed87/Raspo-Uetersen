using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardDeleted : INotification
{
    public required Guid Id { get; set; }
    
    public string IssuedBy { get; set; }
}