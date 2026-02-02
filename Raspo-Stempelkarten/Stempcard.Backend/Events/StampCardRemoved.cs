using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// This event is fired when a stamp card is removed.
/// </summary>
public record StampCardRemoved(Guid Id, string Issuer, DateTimeOffset IssuedOn) : INotification;