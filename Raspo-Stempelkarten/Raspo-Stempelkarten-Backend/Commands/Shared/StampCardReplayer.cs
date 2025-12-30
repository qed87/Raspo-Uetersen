using System.Text.Json;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Model;
using Raspo_Stempelkarten_Backend.Model.Data;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public class StampCardReplayer(string season, string team) : IStampCardReplayer
{
    private readonly StampCardLoadContext _loadContext = new();

    public void Replay(ResolvedEvent[] resolvedEvents)
    {
        foreach (var resolvedEvent in resolvedEvents)
        {
            Replay(resolvedEvent);
        }
    }
    
    public void Replay(ResolvedEvent resolvedEvent)
    {
        var eventCreated = new DateTimeOffset(resolvedEvent.Event.Created.ToUniversalTime(), TimeSpan.Zero);
        if (resolvedEvent.Event.EventType == nameof(StampCardCreated))
        {
            var stampCardCreated = JsonSerializer.Deserialize<StampCardCreated>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardCreated is null) throw new InvalidDataException();
            _loadContext.StampCards.Add(
                new StampCardData
                {
                    Id = stampCardCreated.Id,
                    Recipient = stampCardCreated.Recipient,
                    MaxStamps = stampCardCreated.MaxStamps,
                    MinStamps = stampCardCreated.MinStamps,
                    CreatedBy = stampCardCreated.IssuedBy,
                    Created = eventCreated,
                    Team = team,
                    Season = season
                });
        }

        if (resolvedEvent.Event.EventType == nameof(StampCardPropertyChanged))
        {
            var stampCardPropertyChanged = JsonSerializer.Deserialize<StampCardPropertyChanged>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardPropertyChanged is null) 
                throw new FormatException("StampCardPropertyChanged konnte nicht deserialisiert werden!");
            var stampCard = _loadContext.StampCards.Single(stampCardData => stampCardData.Id == stampCardPropertyChanged.StampCardId);
            if (stampCard is null) throw new InvalidOperationException($"Stempelkarte '{stampCardPropertyChanged.StampCardId}' konnte nicht gefunden werden!");
            switch (stampCardPropertyChanged.Name)
            {
                case nameof(StampCard.Recipient):
                    stampCard.Recipient = (string) stampCardPropertyChanged.Value!;
                    break;
                case nameof(StampCard.MaxStamps):
                    stampCard.MaxStamps = (int) stampCardPropertyChanged.Value!;
                    break;
                case nameof(StampCard.MinStamps):
                    stampCard.MinStamps = (int) stampCardPropertyChanged.Value!;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(stampCardPropertyChanged.Name);
            }

            stampCard.Updated = eventCreated;
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardOwnerAdded))
        {
            var stampCardOwnerAdded = JsonSerializer.Deserialize<StampCardOwnerAdded>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardOwnerAdded is null) throw new FormatException("StampCardOwnerAdded konnte nicht deserialisiert werden!");
            var stampCard = _loadContext.StampCards.Single(stampCardData => stampCardData.Id == stampCardOwnerAdded.StampCardId);
            if (stampCard is null) 
                throw new InvalidOperationException($"Stempelkarte '{stampCardOwnerAdded.StampCardId}' konnte nicht gefunden werden!");
            stampCard.Owners.Add(stampCardOwnerAdded.Name);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardOwnerRemoved))
        {
            var stampCardOwnerRemoved = JsonSerializer.Deserialize<StampCardOwnerRemoved>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardOwnerRemoved is null) throw new FormatException("StampCardOwnerRemoved konnte nicht deserialisiert werden!");
            var stampCard = _loadContext.StampCards.Single(stampCardData => stampCardData.Id == stampCardOwnerRemoved.StampCardId);
            if (stampCard is null) 
                throw new InvalidOperationException($"Stempelkarte '{stampCardOwnerRemoved.StampCardId}' konnte nicht gefunden werden!");
            stampCard.Owners.Remove(stampCardOwnerRemoved.Name);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardDeleted))
        {
            var stampCardDeleted = JsonSerializer.Deserialize<StampCardDeleted>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardDeleted is null) throw new FormatException("StampCardOwnerRemoved konnte nicht deserialisiert werden!");
            var stampCard = _loadContext.StampCards.Single(stampCardData => stampCardData.Id == stampCardDeleted.Id);
            _loadContext.StampCards.Remove(stampCard);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardStamped))
        {
            var stampCardStamped = JsonSerializer.Deserialize<StampCardStamped>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardStamped is null) throw new InvalidDataException();
            var stampCard = _loadContext.StampCards.Single(stampCardData => stampCardData.Id == stampCardStamped.StampCardId);
            if (stampCard is null) throw new InvalidOperationException();
            _loadContext.Stamps.Add(
                new StampData
                {
                    Id = stampCardStamped.Id,
                    StampCardId = stampCard.Id,
                    IssuedBy = stampCardStamped.IssuedBy,
                    Reason = stampCardStamped.Reason
                });
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardStampErased))
        {
            var stampCardStampErased = JsonSerializer.Deserialize<StampCardStampErased>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardStampErased is null) throw new InvalidDataException();
            var stamp = _loadContext.Stamps.Single(stampData => stampData.Id == stampCardStampErased.Id && stampData.StampCardId == stampCardStampErased.StampCardId);
            _loadContext.Stamps.Remove(stamp);
        }
    }

    public StampCardAggregate GetModel()
    {
        return new StampCardAggregate(season, team, _loadContext);
    }
}