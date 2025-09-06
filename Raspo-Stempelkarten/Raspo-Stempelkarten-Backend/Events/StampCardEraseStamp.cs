using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardStampErased : INotification
{
    public Guid Id { get; set; }
    
    public required Guid StampCardId { get; set; }
    
    public required string IssuedBy { get; set; } = null!;
}