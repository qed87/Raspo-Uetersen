using System.Text.Json;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Exceptions;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public class StampModelReplayer(string streamId) : IStampReplayer
{
    public string StreamId { get; } = streamId;
    
    private readonly StampModel _stampModel = new();

    public void Replay(ResolvedEvent[] resolvedEvents)
    {
        foreach (var resolvedEvent in resolvedEvents)
        {
            Replay(resolvedEvent);
        }
    }

    public void Replay(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.EventType == nameof(PlayerAdded))
        {
            var playerAddedEvent = JsonSerializer.Deserialize<PlayerAdded>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (playerAddedEvent is null) throw new ModelLoadException();
            var playerToAdd = _stampModel.Players.SingleOrDefault(player => playerAddedEvent.Id.Equals(player.Id));
            if (playerToAdd is not null)
            {
                playerToAdd.Deleted = false;
            }
            else
            {
                _stampModel.Players.Add(
                    new Player(
                        playerAddedEvent.Id, 
                        playerAddedEvent.FirstName, 
                        playerAddedEvent.Surname, 
                        playerAddedEvent.Birthdate));    
            }
        }
        
        if (resolvedEvent.Event.EventType == nameof(PlayerDeleted))
        {
            var playerDeletedEvent = JsonSerializer.Deserialize<PlayerDeleted>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (playerDeletedEvent is null) throw new ModelLoadException();
            var playerToDelete = _stampModel.Players.Single(player => playerDeletedEvent.Id.Equals(player.Id));
            playerToDelete.Deleted = true;
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardAdded))
        {
            var stampCardAddedEvent = JsonSerializer.Deserialize<StampCardAdded>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardAddedEvent is null) throw new ModelLoadException();
            _stampModel.Cards.Add(new StampCard(stampCardAddedEvent.Id, stampCardAddedEvent.IssuedTo, stampCardAddedEvent.IssuedAt, stampCardAddedEvent.AccountingYear));
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampCardRemoved))
        {
            var stampCardRemovedEvent = JsonSerializer.Deserialize<StampCardRemoved>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampCardRemovedEvent is null) throw new ModelLoadException();
            var stampCard = _stampModel.Cards.SingleOrDefault(card => card.Id.Equals(stampCardRemovedEvent.Id));
            if(stampCard is null) throw new ModelLoadException();
            _stampModel.Cards.Remove(stampCard);
        }
        
        if (resolvedEvent.Event.EventType == nameof(StampAdded))
        {
            var stampAdded = JsonSerializer.Deserialize<StampAdded>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (stampAdded is null) throw new ModelLoadException();
            var stampCard = _stampModel.Cards.SingleOrDefault(card => card.Id.Equals(stampAdded.StampCardId));
            if(stampCard is null) throw new ModelLoadException();
            stampCard.Stamps.Add(
                new Stamp(stampAdded.Id, stampAdded.Reason, stampAdded.IssuedBy, stampAdded.IssuedAt));
        }
        
        if (resolvedEvent.Event.EventType == nameof(EraseStamp))
        {
            var eraseStamp = JsonSerializer.Deserialize<EraseStamp.EraseStamp>(
                resolvedEvent.Event.Data.ToArray(), 
                JsonSerializerOptions.Default);
            if (eraseStamp is null) throw new ModelLoadException();
            var stampCard = _stampModel.Cards.SingleOrDefault(card => card.Id.Equals(eraseStamp.StampCardId));
            if(stampCard is null) throw new ModelLoadException();
            var stamp = stampCard.Stamps.SingleOrDefault(stamp => stamp.Id.Equals(eraseStamp.StampId));
            if(stamp is null) throw new ModelLoadException();
            stampCard.Stamps.Remove(stamp);
        }
    }

    public StampModel GetModel()
    {
        return _stampModel;
    }
}