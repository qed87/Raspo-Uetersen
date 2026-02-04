using DispatchR.Abstractions.Notification;

namespace StampCard.Backend.Events;

/// <summary>
/// The event notification.
/// </summary>
public record IEventNotification : INotification
{
    /// <summary>
    /// The event version.
    /// </summary>
    public string EventVersion { get; set; } = "v1";
}