using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public record PlayerDeleted(Guid Id) : INotification;