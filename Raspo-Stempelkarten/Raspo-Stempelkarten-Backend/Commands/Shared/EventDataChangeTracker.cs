using System.Collections.Concurrent;
using System.Text.Json;
using DispatchR.Abstractions.Notification;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public class EventDataChangeTracker : IEventDataChangeTracker, 
    INotificationHandler<PlayerAdded>, 
    INotificationHandler<PlayerDeleted>, 
    INotificationHandler<TeamAdded>, 
    INotificationHandler<StampCardAdded>,
    INotificationHandler<StampCardRemoved>,
    INotificationHandler<StampAdded>,
    INotificationHandler<StampErased>
{
    private readonly ConcurrentBag<EventData> _changes = [];
    
    public ValueTask Handle(PlayerAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(PlayerAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(PlayerDeleted request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(PlayerDeleted), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(TeamAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(TeamAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }
    
    public ValueTask Handle(StampCardAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampCardAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampCardRemoved request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampCardRemoved), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    public ValueTask Handle(StampErased request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampErased), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }
    
    public IReadOnlyList<EventData> GetChanges()
    {
        return _changes.ToArray();
    }
}