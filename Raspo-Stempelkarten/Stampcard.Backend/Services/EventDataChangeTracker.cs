using System.Collections.Concurrent;
using System.Text.Json;
using DispatchR.Abstractions.Notification;
using KurrentDB.Client;
using StampCard.Backend.Events;

namespace StampCard.Backend.Services;

/// <summary>
/// Event change tracker.
/// </summary>
public sealed class EventDataChangeTracker(ILogger<EventDataChangeTracker> logger) : 
    IEventDataChangeTracker, 
    INotificationHandler<PlayerAdded>, 
    INotificationHandler<PlayerRemoved>, 
    INotificationHandler<TeamAdded>, 
    INotificationHandler<TeamDeleted>,
    INotificationHandler<StampCardAdded>,
    INotificationHandler<StampCardRemoved>,
    INotificationHandler<StampAdded>,
    INotificationHandler<StampErased>,
    INotificationHandler<CoachAdded>,
    INotificationHandler<CoachRemoved>,
    INotificationHandler<TeamUpdated>
{
    private readonly ConcurrentBag<EventData> _changes = [];

    /// <inheritdoc />
    public ValueTask Handle(PlayerAdded request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Player added '{FirstName}' and '{LastName}'.", request.FirstName, request.LastName);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(PlayerAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(PlayerRemoved request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Player removed '{PlayerId}'.", request.Id);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(PlayerRemoved), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(TeamAdded request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Team added '{Name}'.", request.Name);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(TeamAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampCardAdded request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Stamp Card added '{PlayerId}' and {AccountingYear}.", request.PlayerId, request.AccountingYear);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampCardAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampCardRemoved request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Stamp Card removed '{StampCardId}'.", request.Id);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampCardRemoved), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampAdded request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Stamp added '{StampId}' and '{StampCardId}'.", request.Id, request.StampCardId);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(StampErased request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Stamp erased'{StampId}' and '{StampCardId}'.", request.Id, request.StampCardId);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(StampErased), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }
    
    /// <inheritdoc />
    public ValueTask Handle(TeamDeleted request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Team deleted.");
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(TeamDeleted), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(CoachAdded request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Coach added '{Name}'.", request.Email);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(CoachAdded), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask Handle(CoachRemoved request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Coach removed '{Name}'.", request.Email);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(CoachRemoved), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }
    
    /// <inheritdoc />
    public ValueTask Handle(TeamUpdated request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Track change to model: Team name updated to '{Name}'.", request.Name);
        _changes.Add(new EventData(Uuid.NewUuid(), nameof(TeamUpdated), JsonSerializer.SerializeToUtf8Bytes(request)));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public IReadOnlyList<EventData> GetChanges()
    {
        return _changes.ToArray();
    }
}