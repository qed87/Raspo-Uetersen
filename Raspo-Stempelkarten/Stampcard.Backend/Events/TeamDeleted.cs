namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when an existing team is deleted.
/// </summary>
public record TeamDeleted(string Issuer, DateTimeOffset IssuedOn) : IEventNotification;