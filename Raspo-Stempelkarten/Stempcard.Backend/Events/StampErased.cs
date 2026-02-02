using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// This event is fired when a stamp is erased from a stamp card.
/// </summary>
public record StampErased(Guid Id, Guid StampCardId, string Issuer, DateTimeOffset IssuedOn) : INotification;