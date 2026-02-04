namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a new coach is added to a team.
/// </summary>
public record CoachAdded(string Email, string Issuer, DateTimeOffset IssuedOn) : IEventNotification;