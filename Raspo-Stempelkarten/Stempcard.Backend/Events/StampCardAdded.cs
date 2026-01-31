using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public record StampCardAdded(Guid Id, short AccountingYear, Guid PlayerId, DateTimeOffset IssuedDate) : INotification;