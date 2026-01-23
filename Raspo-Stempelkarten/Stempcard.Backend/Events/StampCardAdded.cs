using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardAdded : INotification
{
    public Guid Id { get; set; }
    
    public short AccountingYear { get; set; }
    
    public Guid IssuedTo { get; set; }
    
    public DateTimeOffset IssuedAt { get; set; }
}