namespace StampCard.Backend.Events;

/// <summary>
/// This event is fired when a new stamp is added to a member stamp card.
/// </summary>
public record StampAdded(Guid Id, Guid StampCardId, string Reason, 
    string Issuer, DateTimeOffset IssuedOn) : IEventNotification;