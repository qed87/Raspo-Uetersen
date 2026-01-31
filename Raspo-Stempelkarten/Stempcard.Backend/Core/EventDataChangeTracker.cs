using System.Collections.Concurrent;
using System.Text.Json;
using DispatchR.Abstractions.Notification;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Core;

/// <summary>
/// Event change tracker.
/// </summary>
public sealed class EventDataChangeTracker : 
    IEventDataChangeTracker, 
    INotificationHandler<PlayerAdded>, 
    INotificationHandler<PlayerDeleted>, 
    INotificationHandler<TeamAdded>, 
    INotificationHandler<TeamDeleted>,
    INotificationHandler<StampCardAdded>,
    INotificationHandler<StampCardRemoved>,
    INotificationHandler<StampAdded>,
    INotificationHandler<StampErased>,
    INotificationHandler<CoachAdded>,
    INotificationHandler<CoachRemoved>
    
{
    private readonly ConcurrentBag<EventData> _changes = [];

    /// <inheritdoc />
    public ValueTask Handle(PlayerAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(PlayerAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(PlayerDeleted request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(PlayerDeleted), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(TeamAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(TeamAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampCardAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampCardAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampCardRemoved request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampCardRemoved), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampErased request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampErased), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }
    
    /// <inheritdoc />
    public ValueTask Handle(TeamDeleted request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(TeamDeleted), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(CoachAdded request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(CoachAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(CoachRemoved request, CancellationToken cancellationToken)
    {
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(CoachRemoved), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public IReadOnlyList<EventData> GetChanges()
    {
        return _changes.ToArray();
    }
}