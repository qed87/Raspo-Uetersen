using System.Text.Json;
using KurrentDB.Client;
using Raspo_Stempelkarten_Backend.Events;
using Raspo_Stempelkarten_Backend.Exceptions;
using Raspo_Stempelkarten_Backend.Model;

namespace Raspo_Stempelkarten_Backend.Services;

/// <summary>
/// Replays a team model.
/// </summary>
public class TeamModelReplayer(Team team) : ITeamModelReplayer
{
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
            case nameof(TeamAdded):
            {
                var teamAdded = JsonSerializer.Deserialize<TeamAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (teamAdded is null) throw ModelReadError("TeamAdded");
                team.Name = teamAdded.Name;
                team.Club = teamAdded.Club;
                team.CreatedBy = teamAdded.Issuer;
                team.CreatedOn = teamAdded.IssuedOn;
                break;
            }
            case nameof(CoachAdded):
            {
                var coachAdded = JsonSerializer.Deserialize<CoachAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (coachAdded is null) throw ModelReadError("CoachAdded");
                team.Coaches.Add(
                    new Coach { Email = coachAdded.Email, Issuer = coachAdded.Issuer, IssuedOn = coachAdded.IssuedOn});
                break;
            }
            case nameof(CoachRemoved):
            {
                var coachAdded = JsonSerializer.Deserialize<CoachRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (coachAdded is null) throw ModelReadError("CoachRemoved");
                team.Coaches.RemoveAll(coach => coach.Email == coachAdded.Email);
                break;
            }
            case nameof(MemberAdded):
            {
                var playerAddedEvent = JsonSerializer.Deserialize<MemberAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (playerAddedEvent is null) throw ModelReadError("PlayerAdded");
                var playerToAdd = team.Members.SingleOrDefault(player => playerAddedEvent.Id.Equals(player.Id));
                if (playerToAdd is not null)
                {
                    playerToAdd.Deleted = false;
                }
                else
                {
                    team.Members.Add(
                        new Member(
                            playerAddedEvent.Id, 
                            playerAddedEvent.FirstName, 
                            playerAddedEvent.LastName, 
                            playerAddedEvent.Birthdate,
                            playerAddedEvent.Birthplace));    
                }

                break;
            }
            case nameof(MemberRemoved):
            {
                var playerDeletedEvent = JsonSerializer.Deserialize<MemberRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (playerDeletedEvent is null) throw ModelReadError("PlayerDeleted");
                var playerToDelete = team.Members.Single(player => playerDeletedEvent.Id.Equals(player.Id));
                playerToDelete.Deleted = true;
                break;
            }
            case nameof(StampCardAdded):
            {
                var stampCardAddedEvent = JsonSerializer.Deserialize<StampCardAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampCardAddedEvent is null) throw ModelReadError("StampCardAdded");
                team.Cards.Add(new StampCard(
                    stampCardAddedEvent.Id, 
                    stampCardAddedEvent.MemberId, 
                    stampCardAddedEvent.Issuer,
                    stampCardAddedEvent.IssuedOn, 
                    stampCardAddedEvent.AccountingYear));
                break;
            }
            case nameof(StampCardRemoved):
            {
                var stampCardRemovedEvent = JsonSerializer.Deserialize<StampCardRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampCardRemovedEvent is null) throw ModelReadError("StampCardRemoved");
                var stampCard = team.Cards.SingleOrDefault(card => card.Id.Equals(stampCardRemovedEvent.Id));
                if(stampCard is null) throw ModelReadError("Stempelkarte");
                team.Cards.Remove(stampCard);
                break;
            }
            case nameof(StampAdded):
            {
                var stampAdded = JsonSerializer.Deserialize<StampAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampAdded is null) throw ModelReadError("StampAdded");
                var stampCard = team.Cards.SingleOrDefault(card => card.Id.Equals(stampAdded.StampCardId));
                if(stampCard is null) throw new ModelLoadException("StampCard");
                stampCard.Stamps.Add(
                    new Stamp(stampAdded.Id, stampAdded.Reason, stampAdded.Issuer, stampAdded.IssuedOn));
                break;
            }
            case nameof(Commands.EraseStamp):
            {
                var eraseStamp = JsonSerializer.Deserialize<Commands.EraseStamp.EraseStampCommand>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (eraseStamp is null) throw ModelReadError("EraseStamp");
                var stampCard = team.Cards.SingleOrDefault(card => card.Id.Equals(eraseStamp.StampCardId));
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
        return team;
    }
}