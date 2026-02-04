namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a coach is removed from a team.
/// </summary>
public record CoachRemoved(string Email, string Issuer, DateTimeOffset IssuedOn) : IEventNotification;