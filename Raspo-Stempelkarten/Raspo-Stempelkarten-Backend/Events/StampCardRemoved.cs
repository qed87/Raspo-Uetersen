using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardRemoved : INotification
{
    public Guid Id { get; set; }
    public Guid StampCardId { get; set; }
}