using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// This event is fired when an existing team is deleted.
/// </summary>
public record TeamDeleted(string Id, string Issuer, DateTimeOffset IssuedOn) : INotification;