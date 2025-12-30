using System.Collections.Concurrent;
using System.Text.Json;
using DispatchR.Abstractions.Notification;
using JetBrains.Annotations;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

[UsedImplicitly]
public class StampCardChangeHandler 
    : IStampCardChangeTracker, INotificationHandler<StampCardCreated>, INotificationHandler<StampCardDeleted>, 
        INotificationHandler<StampCardPropertyChanged>, INotificationHandler<StampCardOwnerAdded>,
        INotificationHandler<StampCardOwnerRemoved>, INotificationHandler<StampCardStamped>, 
        INotificationHandler<StampCardStampErased>
{
    private readonly ConcurrentQueue<EventData> _changes = [];
 
    public IEnumerable<EventData> GetChanges() => _changes.ToList();
    
    public ValueTask Handle(StampCardCreated message, CancellationToken cancellationToken)
    {
        _changes.Enqueue(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardCreated), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampCardDeleted message, CancellationToken cancellationToken)
    {
        _changes.Enqueue(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardDeleted), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampCardPropertyChanged message, CancellationToken cancellationToken)
    {
        _changes.Enqueue(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardPropertyChanged), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampCardOwnerAdded message, CancellationToken cancellationToken)
    {
        _changes.Enqueue(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardOwnerAdded), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampCardOwnerRemoved message, CancellationToken cancellationToken)
    {
        _changes.Enqueue(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardOwnerRemoved), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampCardStamped message, CancellationToken cancellationToken)
    {
        _changes.Enqueue(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardStamped), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampCardStampErased message, CancellationToken cancellationToken)
    {
        _changes.Enqueue(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardStampErased), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return ValueTask.CompletedTask;
    }
}