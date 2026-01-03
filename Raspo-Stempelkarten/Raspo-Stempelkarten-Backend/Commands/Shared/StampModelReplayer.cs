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
    }

    public StampModel GetModel()
    {
        return _stampModel;
    }
}