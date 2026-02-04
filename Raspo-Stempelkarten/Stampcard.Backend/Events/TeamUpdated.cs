namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a team is updated.
/// </summary>
public record TeamUpdated(string Id, string Name, string Issuer, DateTimeOffset IssuedOn) : IEventNotification;