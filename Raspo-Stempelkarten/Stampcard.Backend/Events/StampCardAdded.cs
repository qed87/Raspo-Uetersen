namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a new member stamp card is added to a team. 
/// </summary>
public record StampCardAdded(Guid Id, short AccountingYear, Guid PlayerId, 
    string Issuer, DateTimeOffset IssuedOn) : IEventNotification;