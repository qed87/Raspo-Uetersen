namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a new player is added to the team.
/// </summary>
public record PlayerAdded(Guid Id, string FirstName, string LastName, DateOnly Birthdate, string Birthplace, 
    string Issuer, DateTimeOffset IssuedOn) : IEventNotification;