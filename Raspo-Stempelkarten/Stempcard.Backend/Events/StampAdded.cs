using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public record StampAdded(Guid Id, Guid StampCardId, string Reason, string IssuedBy, DateTimeOffset IssuedDate) : INotification;