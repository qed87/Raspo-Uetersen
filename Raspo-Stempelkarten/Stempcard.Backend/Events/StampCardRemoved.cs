using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public record StampCardRemoved(Guid Id) : INotification;