using FluentResults;

namespace Raspo_Stempelkarten_Backend.Model;

public class StampModel : IStampModel
{
    public ulong? Version { get; set; }

    public List<Player> Players { get; } = [];
    
    public List<StampCard> Cards { get; } = [];
    
    public Result<Guid> AddPlayer(string firstName, string surname, DateOnly birthdate)
    {
        if(string.IsNullOrEmpty(firstName)) return Result.Fail("First name is empty");
        if(string.IsNullOrEmpty(surname)) return Result.Fail("Surname is empty");
        if(birthdate > DateOnly.FromDateTime(DateTime.Now)) return Result.Fail("Date of birth must not be in the future");
        var newPlayer = new Player(Guid.NewGuid(), firstName, surname, birthdate);
        var playerFound = Players.SingleOrDefault(player => player.Equals(newPlayer));
        if (playerFound is not null && playerFound.Deleted)
        {
            // reactivate player
            playerFound.Deleted = false;
            return Result.Ok(playerFound.Id);
        }
        
        if (playerFound is not null && !playerFound.Deleted)
        {
            return Result.Fail("Player already exist");
        }
        
        Players.Add(newPlayer);
        return Result.Ok(newPlayer.Id);
    }
    
    public Result<Guid> DeletePlayer(Guid playerId)
    {
        var playerFound = Players.SingleOrDefault(player => player.Id.Equals(playerId));
        if (playerFound is null) return Result.Fail("Player not found");
        playerFound.Deleted = true;
        return Result.Ok(playerFound.Id);
    }

    public Result<StampCard> AddStampCard(Guid issuedTo, short accountingYear)
    {
        var playerResponse = GetActivePlayerById(issuedTo);
        if(!playerResponse.IsSuccess) return playerResponse.ToResult();
        var newStampCard = new StampCard(playerResponse.Value.Id, accountingYear);
        var cardFound = Cards.SingleOrDefault(stampCard => stampCard.Equals(newStampCard));
        if (cardFound is not null) return Result.Fail("Card already exist");
        Cards.Add(newStampCard);
        return Result.Ok(newStampCard);
    }

    public Result<Stamp> AddStamp(Guid stampCardId, string reason)
    {
        var stampCard = Cards.SingleOrDefault(card => card.Id == stampCardId);
        if(stampCard is null) return Result.Fail("StampCard not found");
        var newStamp = new Stamp(reason);
        stampCard.Stamps.Add(newStamp);
        return Result.Ok(newStamp);
    }
    
    public Result<Stamp> EraseStamp(Guid stampCardId, Guid stampId)
    {
        var stampCard = Cards.SingleOrDefault(card => card.Id == stampCardId);
        if(stampCard is null) return Result.Fail("StampCard not found");
        var stamp = stampCard.Stamps.SingleOrDefault(stamp => stamp.Id == stampId);
        if(stamp is null) return Result.Fail("Stamp not found");
        stampCard.Stamps.Remove(stamp);
        return Result.Ok(stamp);
    }

    public Result<Guid> DeleteStampCard(Guid id)
    {
        var stampCard = Cards.SingleOrDefault(card => card.Id == id);
        if(stampCard is null) return Result.Fail("StampCard not found");
        Cards.Remove(stampCard);
        return Result.Ok(stampCard.Id);
    }

    private Result<Player> GetActivePlayerById(Guid playerId)
    {
        var playerFound = Players.SingleOrDefault(player => player.Id.Equals(playerId) && !player.Deleted);
        return playerFound is null 
            ? Result.Fail($"No player found with Id = {playerId}") 
            : Result.Ok(playerFound);
    }
}