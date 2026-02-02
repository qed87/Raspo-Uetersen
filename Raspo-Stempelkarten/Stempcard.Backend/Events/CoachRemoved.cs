using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// This event is fired when a coach is removed from a team.
/// </summary>
public record CoachRemoved(string Email, string Issuer, DateTimeOffset IssuedOn) : INotification;