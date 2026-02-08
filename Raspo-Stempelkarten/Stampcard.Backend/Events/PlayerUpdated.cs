namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a existing player is updated.
/// </summary>
public record PlayerUpdated(Guid Id, string FirstName, string LastName, DateOnly Birthdate, string Birthplace, bool Active,
    string Issuer, DateTimeOffset IssuedOn) : IEventNotification;