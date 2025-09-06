using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardStamped : INotification
{
    public required Guid Id { get; set; }
    
    public required Guid StampCardId { get; set; }
    
    public required string IssuedBy { get; set; } = null!;
    
    public string? Reason { get; set; }
}