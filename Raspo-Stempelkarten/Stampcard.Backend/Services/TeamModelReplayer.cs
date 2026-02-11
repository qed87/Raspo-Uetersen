using System.Text.Json;
using KurrentDB.Client;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using StampCard.Backend.Events;
using StampCard.Backend.Exceptions;
using StampCard.Backend.Model;
using StampCard.Backend.Services.Interfaces;

namespace StampCard.Backend.Services;

/// <summary>
/// Replays a team model.
/// </summary>
public class TeamModelReplayer(ILogger<TeamModelReplayer> logger, Team team) : ITeamModelReplayer
{
    /// <inheritdoc />
    public void Replay(ResolvedEvent[] resolvedEvents)
    {
        foreach (var resolvedEvent in resolvedEvents)
        {
            logger.LogTrace("Replay resolved event ({EventType})) from stream '{StreamName}' at global-position={Position}, stream-position={EventNumber}.",
                resolvedEvent.Event.EventType,
                resolvedEvent.OriginalStreamId,
                resolvedEvent.OriginalPosition.GetValueOrDefault(),
                resolvedEvent.OriginalEventNumber.ToUInt64());
            Replay(resolvedEvent);
        }
    }

    /// <inheritdoc />
    public void Replay(ResolvedEvent resolvedEvent)
    {
        switch (resolvedEvent.Event.EventType)
        {
            case nameof(TeamDeleted):
                var teamDeleted = JsonSerializer.Deserialize<TeamDeleted>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (teamDeleted is null) throw ModelReadError("TeamDeleted");
                logger.LogTrace("Replayed [TeamDeleted] Id = {Id}", team.Id);
                team.Deleted = true;
                break;
            case nameof(TeamUpdated):
                var teamUpdated = JsonSerializer.Deserialize<TeamUpdated>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (teamUpdated is null) throw ModelReadError("TeamUpdated");
                logger.LogTrace("Replayed [TeamUpdated] Id = {Id}", team.Id);
                team.Name = teamUpdated.Name;
                team.LastUpdatedBy = teamUpdated.Issuer;
                team.LastModified = teamUpdated.IssuedOn;
                break;
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
                logger.LogTrace("Replayed [TeamAdded]: Club={Club}, Name={Name}.", 
                    teamAdded.Club, teamAdded.Name);
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
                logger.LogTrace("Replayed [CoachAdded]: Email={Email}.", 
                    coachAdded.Email);
                break;
            }
            case nameof(CoachRemoved):
            {
                var coachRemoved = JsonSerializer.Deserialize<CoachRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (coachRemoved is null) throw ModelReadError("CoachRemoved");
                team.Coaches.RemoveAll(coach => coach.Email == coachRemoved.Email);
                logger.LogTrace("Replayed [CoachRemoved]: Email={Email}.", 
                    coachRemoved.Email);
                break;
            }
            case nameof(PlayerAdded):
            {
                var playerAdded = JsonSerializer.Deserialize<PlayerAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (playerAdded is null) throw ModelReadError("PlayerAdded");
                var playerToAdd = team.Players.SingleOrDefault(player => playerAdded.Id.Equals(player.Id));
                if (playerToAdd is not null) throw ModelReadError("PlayerAdded");
                team.Players.Add(
                    new Player(
                        playerAdded.Id, 
                        playerAdded.FirstName, 
                        playerAdded.LastName, 
                        playerAdded.Birthdate,
                        playerAdded.Birthplace));    
                logger.LogTrace("Replayed [PlayerAdded]: Id='{Id}', FirstName='{FirstName}', LastName='{LastName}'.", 
                    playerAdded.Id, playerAdded.FirstName, playerAdded.LastName);
                break;
            }
            case nameof(PlayerUpdated):
            {
                var playerUpdated = JsonSerializer.Deserialize<PlayerUpdated>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (playerUpdated is null) throw ModelReadError("PlayerUpdated");
                var playerToUpdate = team.Players.SingleOrDefault(player => playerUpdated.Id.Equals(player.Id));
                if (playerToUpdate is null) throw ModelReadError("PlayerUpdated");
                playerToUpdate.FirstName = playerUpdated.FirstName;
                playerToUpdate.LastName = playerUpdated.LastName;
                playerToUpdate.Birthdate = playerUpdated.Birthdate;
                playerToUpdate.Birthplace = playerUpdated.Birthplace;
                playerToUpdate.Active = playerUpdated.Active;
                logger.LogTrace("Replay [PlayerUpdated]: Player with Id {Id}.", playerUpdated.Id);
                break;
            }
            case nameof(PlayerRemoved):
            {
                var playerDeleted = JsonSerializer.Deserialize<PlayerRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (playerDeleted is null) throw ModelReadError("PlayerDeleted");
                team.Players.RemoveAll(player => player.Id.Equals(playerDeleted.Id));
                team.Cards.RemoveAll(card => card.PlayerId.Equals(playerDeleted.Id));
                logger.LogTrace("Replay [PlayerDeleted]: Id='{Id}'.", playerDeleted.Id);
                break;
            }
            case nameof(StampCardAdded):
            {
                var stampCardAdded = JsonSerializer.Deserialize<StampCardAdded>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampCardAdded is null) throw ModelReadError("StampCardAdded");
                team.Cards.Add(new Model.StampCard(
                    stampCardAdded.Id, 
                    stampCardAdded.PlayerId, 
                    stampCardAdded.Issuer,
                    stampCardAdded.IssuedOn, 
                    stampCardAdded.AccountingYear));
                logger.LogTrace("Replay [StampCardAdded]: Id='{Id}', StampCardId='{StampCardId}', AccountingYear={AccountingYear}.", 
                    stampCardAdded.Id, stampCardAdded.PlayerId, stampCardAdded.AccountingYear);
                break;
            }
            case nameof(StampCardRemoved):
            {
                var stampCardRemoved = JsonSerializer.Deserialize<StampCardRemoved>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (stampCardRemoved is null) throw ModelReadError("StampCardRemoved");
                var stampCard = team.Cards.SingleOrDefault(card => card.Id.Equals(stampCardRemoved.Id));
                if(stampCard is null) throw ModelReadError("Stempelkarte");
                team.Cards.Remove(stampCard);
                logger.LogTrace("Replay [StampCardRemoved]: Id='{Id}'.", 
                    stampCardRemoved.Id);
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
                logger.LogTrace("Replay [StampAdded]: Id='{Id}', StampCardId='{StampCardId}'.", 
                    stampAdded.Id, stampAdded.StampCardId);
                break;
            }
            case nameof(StampErased):
            {
                var eraseStamp = JsonSerializer.Deserialize<StampErased>(
                    resolvedEvent.Event.Data.ToArray(), 
                    JsonSerializerOptions.Default);
                if (eraseStamp is null) throw ModelReadError("EraseStamp");
                var stampCard = team.Cards.SingleOrDefault(card => card.Id.Equals(eraseStamp.StampCardId));
                if(stampCard is null) throw ModelReadError("StampCard");
                var stamp = stampCard.Stamps.SingleOrDefault(stamp => stamp.Id.Equals(eraseStamp.Id));
                if(stamp is null) throw ModelReadError("Stamp");
                stampCard.Stamps.Remove(stamp);
                logger.LogTrace("Replay [EraseStamp]: Id='{Id}', StampCardId='{StampCardId}'.", 
                    eraseStamp.Id, eraseStamp.StampCardId);
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