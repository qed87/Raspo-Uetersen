namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a stamp is erased from a stamp card.
/// </summary>
public record StampErased(Guid Id, Guid StampCardId, string Issuer, DateTimeOffset IssuedOn) : IEventNotification;