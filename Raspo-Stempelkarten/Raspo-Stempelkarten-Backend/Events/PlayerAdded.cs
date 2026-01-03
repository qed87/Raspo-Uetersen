using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

public record PlayerAdded(Guid Id, string FirstName, string Surname, DateOnly Birthdate) : INotification;