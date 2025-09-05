using System.Text.Json;
using JetBrains.Annotations;
using KurrentDB.Client;
using LiteBus.Events.Abstractions;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

[UsedImplicitly]
public class StampCardChangeTracker : IEventHandler<StampCardCreated>, 
    IEventHandler<StampCardDeleted>, IEventHandler<StampCardPropertyChanged>, IEventHandler<StampCardOwnerAdded>,
    IEventHandler<StampCardOwnerRemoved>, IEventHandler<StampCardStamped>, IEventHandler<StampCardStampErased>
{
    private bool _enabled;
    private readonly List<EventData> _changes = [];
    
    public void Enable()
    {
        _enabled = true;
    }
    
    public void Disable()
    {
        _enabled = false;
    }
 
    public Task HandleAsync(StampCardCreated message, CancellationToken cancellationToken = new())
    {
        if (!_enabled) return Task.CompletedTask;
        _changes.Add(
            new EventData(
                Uuid.NewUuid(), 
            nameof(StampCardCreated), 
            JsonSerializer.SerializeToUtf8Bytes(message)));
        return Task.CompletedTask;
    }
    
    public Task HandleAsync(StampCardPropertyChanged message, CancellationToken cancellationToken = new())
    {
        if (!_enabled) return Task.CompletedTask;
        _changes.Add(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardPropertyChanged), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return Task.CompletedTask;
    }

    public Task HandleAsync(StampCardDeleted message, CancellationToken cancellationToken = new())
    {
        if (!_enabled) return Task.CompletedTask;
        _changes.Add(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardDeleted), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return Task.CompletedTask;
    }

    public Task HandleAsync(StampCardOwnerAdded message, CancellationToken cancellationToken = new())
    {
        if (!_enabled) return Task.CompletedTask;
        _changes.Add(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardOwnerAdded), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return Task.CompletedTask;
    }

    public Task HandleAsync(StampCardOwnerRemoved message, CancellationToken cancellationToken = new())
    {
        if (!_enabled) return Task.CompletedTask;
        _changes.Add(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardOwnerRemoved), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return Task.CompletedTask;
    }

    public Task HandleAsync(StampCardStamped message, CancellationToken cancellationToken = new())
    {
        if (!_enabled) return Task.CompletedTask;
        _changes.Add(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardStamped), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return Task.CompletedTask;
    }

    public Task HandleAsync(StampCardStampErased message, CancellationToken cancellationToken = new())
    {
        if (!_enabled) return Task.CompletedTask;
        _changes.Add(
            new EventData(
                Uuid.NewUuid(), 
                nameof(StampCardStampErased), 
                JsonSerializer.SerializeToUtf8Bytes(message)));
        return Task.CompletedTask;
    }

    public object Handle(object message)
    {
        return HandleAsync((dynamic)message);
    }

    public IEnumerable<EventData> GetChanges() => _changes.ToList();
}