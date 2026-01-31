using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// TeamAdded event. 
/// </summary>
public record TeamAdded(string Club, string Name, string IssuedBy, DateTimeOffset IssuedDate) : INotification;