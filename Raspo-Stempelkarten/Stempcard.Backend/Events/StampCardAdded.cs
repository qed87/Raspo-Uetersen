using DispatchR.Abstractions.Notification;

namespace Raspo_Stempelkarten_Backend.Events;

/// <summary>
/// This event is fired when a new member stamp card is added to a team. 
/// </summary>
public record StampCardAdded(Guid Id, short AccountingYear, Guid MemberId, 
    string Issuer, DateTimeOffset IssuedOn) : INotification;