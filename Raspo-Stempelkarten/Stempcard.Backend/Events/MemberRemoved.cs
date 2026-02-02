using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// This event is fired when an existing member is removed from the team.
/// </summary>
public record MemberRemoved(Guid Id, string Issuer, DateTimeOffset IssuedOn) : INotification;