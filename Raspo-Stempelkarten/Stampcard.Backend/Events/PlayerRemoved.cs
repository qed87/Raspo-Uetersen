namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when an existing member is removed from the team.
/// </summary>
public record PlayerRemoved(Guid Id, string Issuer, DateTimeOffset IssuedOn) : IEventNotification;