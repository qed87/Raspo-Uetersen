using System.Text.Json;
using DispatchR;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Exceptions;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Core;

/// <summary>
/// 
/// </summary>
/// <param name="streamId"></param>
public class TeamModelReplayer(IMediator mediator, string streamId) : ITeamModelReplayer
{
    /// <inheritdoc />
    public string StreamId { get; } = streamId;
    
    private readonly Team _team = new(mediator);

    /// <inheritdoc />
    public void Replay(ResolvedEvent[] resolvedEvents)
    {
        foreach (var resolvedEvent in resolvedEvents)
        {
            Replay(resolvedEvent);
        }
    }

    /// <inheritdoc />
    public void Replay(ResolvedEvent resolvedEvent)
    {
        switch (resolvedEvent.Event.EventType)
        {
            case nameof(Commands.DeleteTeam):
                throw new ModelLoadException("Gelöschtes Model kann nicht geladen werden.");
            case nameof(CoachAdded):
            {
                var coachAdded = JsonSerializer.Deserialize<CoachAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (coachAdded is null) throw ModelReadError("CoachAdded");
                _team.Coaches.Add(
                    new Coach { Email = coachAdded.Email, IssuedBy = coachAdded.IssuedBy, IssuedDate = coachAdded.IssuedDate});
                break;
            }
            case nameof(CoachRemoved):
            {
                var coachAdded = JsonSerializer.Deserialize<CoachRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (coachAdded is null) throw ModelReadError("CoachRemoved");
                _team.Coaches.RemoveAll(coach => coach.Email == coachAdded.Email);
                break;
            }
            case nameof(PlayerAdded):
            {
                var playerAddedEvent = JsonSerializer.Deserialize<PlayerAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (playerAddedEvent is null) throw ModelReadError("PlayerAdded");
                var playerToAdd = _team.Players.SingleOrDefault(player => playerAddedEvent.Id.Equals(player.Id));
                if (playerToAdd is not null)
                {
                    playerToAdd.Deleted = false;
                }
                else
                {
                    _team.Players.Add(
                        new Player(
                            playerAddedEvent.Id, 
                            playerAddedEvent.FirstName, 
                            playerAddedEvent.LastName, 
                            playerAddedEvent.Birthdate,
                            playerAddedEvent.Birthplace));    
                }

                break;
            }
            case nameof(PlayerDeleted):
            {
                var playerDeletedEvent = JsonSerializer.Deserialize<PlayerDeleted>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (playerDeletedEvent is null) throw ModelReadError("PlayerDeleted");
                var playerToDelete = _team.Players.Single(player => playerDeletedEvent.Id.Equals(player.Id));
                playerToDelete.Deleted = true;
                break;
            }
            case nameof(StampCardAdded):
            {
                var stampCardAddedEvent = JsonSerializer.Deserialize<StampCardAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampCardAddedEvent is null) throw ModelReadError("StampCardAdded");
                _team.Cards.Add(new StampCard(
                    stampCardAddedEvent.Id, 
                    stampCardAddedEvent.PlayerId, 
                    stampCardAddedEvent.IssuedDate, 
                    stampCardAddedEvent.AccountingYear));
                break;
            }
            case nameof(StampCardRemoved):
            {
                var stampCardRemovedEvent = JsonSerializer.Deserialize<StampCardRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampCardRemovedEvent is null) throw ModelReadError("StampCardRemoved");
                var stampCard = _team.Cards.SingleOrDefault(card => card.Id.Equals(stampCardRemovedEvent.Id));
                if(stampCard is null) throw ModelReadError("Stempelkarte");
                _team.Cards.Remove(stampCard);
                break;
            }
            case nameof(StampAdded):
            {
                var stampAdded = JsonSerializer.Deserialize<StampAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampAdded is null) throw ModelReadError("StampAdded");
                var stampCard = _team.Cards.SingleOrDefault(card => card.Id.Equals(stampAdded.StampCardId));
                if(stampCard is null) throw new ModelLoadException("StampCard");
                stampCard.Stamps.Add(
                    new Stamp(stampAdded.Id, stampAdded.Reason, stampAdded.IssuedBy, stampAdded.IssuedDate));
                break;
            }
            case nameof(Commands.EraseStamp):
            {
                var eraseStamp = JsonSerializer.Deserialize<Commands.EraseStamp.EraseStamp>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (eraseStamp is null) throw ModelReadError("EraseStamp");
                var stampCard = _team.Cards.SingleOrDefault(card => card.Id.Equals(eraseStamp.StampCardId));
                if(stampCard is null) throw ModelReadError("StampCard");
                var stamp = stampCard.Stamps.SingleOrDefault(stamp => stamp.Id.Equals(eraseStamp.StampId));
                if(stamp is null) throw ModelReadError("Stamp");
                stampCard.Stamps.Remove(stamp);
                break;
            }
        }
    }

    private static ModelLoadException ModelReadError(string entityName)
    {
        return new ModelLoadException($"Fehler beim Lesen von '{entityName}' vom Stream.");
    }

    /// <inheritdoc />
    public Team GetModel()
    {
        return _team;
    }
}