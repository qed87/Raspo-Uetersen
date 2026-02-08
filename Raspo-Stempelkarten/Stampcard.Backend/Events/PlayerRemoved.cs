namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when an existing player is removed.
/// </summary>
public record PlayerRemoved(Guid Id, string Issuer, DateTimeOffset IssuedOn) : IEventNotification;