using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// Player added event.
/// </summary>
public record PlayerAdded(Guid Id, string FirstName, string LastName, DateOnly Birthdate, string Birthplace) : INotification;