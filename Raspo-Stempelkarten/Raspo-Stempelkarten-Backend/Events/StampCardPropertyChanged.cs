using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardPropertyChanged : INotification
{
    public required Guid StampCardId { get; set; }
    
    public string IssuedBy { get; set; }
    
    public required string Name { get; set; }  = null!;
    
    public required object? Value { get; set; }
    
}