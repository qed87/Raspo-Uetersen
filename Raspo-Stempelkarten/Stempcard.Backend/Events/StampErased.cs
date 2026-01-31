using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public record StampErased(Guid Id, Guid StampCardId) : INotification;