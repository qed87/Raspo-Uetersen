namespace StampCard.Backend.Events;

/// <summary>
/// This event is added when a new team is created and published.
/// </summary>
public record TeamAdded(string Club, string Name, string Issuer, DateTimeOffset IssuedOn) : IEventNotification;