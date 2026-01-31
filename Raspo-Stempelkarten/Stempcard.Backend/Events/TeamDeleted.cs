using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// Team deleted event.
/// </summary>
public record TeamDeleted(string Id) : INotification;