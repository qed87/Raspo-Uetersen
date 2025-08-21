using System.Collections.Immutable;
using System.Text.Json;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Model;

internal class StempelkartenAggregate
{
    private readonly Dictionary<Guid, Stempelkarte> _stempelkarten = [];
    private List<UserEvent> _changes = [];
    private bool _isLoaded = false;

    public string Team { get; set; }
    
    public string Season { get; set; }
    
    public long Position { get; set; }

    public IEnumerable<EventData> GetChanges()
    {
        List<UserEvent> changes = _changes.ToList();
        foreach (var stempelkarte in _stempelkarten.Values)
        {
            changes.AddRange(stempelkarte.GetChanges());
        }
        return changes.Select(@event => new EventData(
            Uuid.NewUuid(), 
            @event.GetType().Name,
            JsonSerializer.SerializeToUtf8Bytes(@event)));
    }
    
    public void Replay(ResolvedEvent[] resolvedEvents)
    {
        foreach (var resolvedEvent in resolvedEvents)
        {
            Replay(resolvedEvent);
        }
    }
    
    public void Replay(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.EventType == nameof(StampCardCreated))
        {
            var stampCardCreated = JsonSerializer.Deserialize<StampCardCreated>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardCreated is null) throw new InvalidDataException();
            var stempelkarte = new Stempelkarte(
                stampCardCreated.Id, 
                stampCardCreated.Recipient, 
                stampCardCreated.Owner,
                stampCardCreated.MaxStamps, 
                stampCardCreated.MinStamps);
            _stempelkarten.Add(stempelkarte.Id, stempelkarte);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardOwnerAdded))
        {
            var stampCardOwnerAdded = JsonSerializer.Deserialize<StampCardOwnerAdded>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardOwnerAdded is null) throw new InvalidDataException();
            var stempelkarte = _stempelkarten[stampCardOwnerAdded.StampCardId];
            stempelkarte.AdditionalOwners.Add(stampCardOwnerAdded.Name);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardOwnerRemoved))
        {
            var stampCardOwnerAdded = JsonSerializer.Deserialize<StampCardOwnerRemoved>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardOwnerAdded is null) throw new InvalidDataException();
            var stempelkarte = _stempelkarten[stampCardOwnerAdded.StampCardId];
            stempelkarte.AdditionalOwners.Remove(stampCardOwnerAdded.Name);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardUpdated))
        {
            var stampCardUpdated = JsonSerializer.Deserialize<StampCardUpdated>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardUpdated is null) throw new InvalidDataException();
            var stempelkarte = _stempelkarten[stampCardUpdated.Id];
            stempelkarte.MaxStamps = stampCardUpdated.MaxStamps;
            stempelkarte.MinStamps = stampCardUpdated.MinStamps;
            stempelkarte.Recipient = stampCardUpdated.Recipient;
            stempelkarte.Update(stampCardUpdated.AdditionalOwners);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardDeleted))
        {
            var stampCardDeleted = JsonSerializer.Deserialize<StampCardDeleted>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardDeleted is null) throw new InvalidDataException();
            _stempelkarten.Remove(stampCardDeleted.Id);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardStamped))
        {
            var stampCardStamped = JsonSerializer.Deserialize<StampCardStamped>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardStamped is null) throw new InvalidDataException();
            var stempelkarte = _stempelkarten[stampCardStamped.Id];
            stempelkarte.Stamp(stampCardStamped.Id, stampCardStamped.IssuedBy, stampCardStamped.Reason);
        }
    }

    public void SetLoaded(StreamPosition? streamPosition)
    {
        Position = streamPosition?.ToInt64() ?? 0;
        _isLoaded = true;
        foreach (var stempelkarte in _stempelkarten.Values)
        {
            stempelkarte.SetLoaded();
        }
    }

    public Stempelkarte AddStempelkarte(
        string recipient, 
        string owner, 
        string[] additionalOwners, 
        int minStamps, 
        int maxStamps)
    {
        var stempelkarte = new Stempelkarte(
            Guid.NewGuid(),  
            recipient, 
            owner, 
            minStamps, 
            maxStamps,
            additionalOwners);
        _changes.Add(new StampCardCreated
        {
            Team = Team, 
            Season = Season, 
            Recipient = recipient, 
            MaxStamps = maxStamps, 
            MinStamps = minStamps
        });
        
        foreach (var additionalOwner in additionalOwners)
            _changes.Add(new StampCardOwnerAdded
            {
                Id = Guid.NewGuid(), 
                StampCardId = stempelkarte.Id, 
                Name = additionalOwner
            });
        return stempelkarte;
    }
}