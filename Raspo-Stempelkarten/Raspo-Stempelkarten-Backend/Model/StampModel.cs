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

    public Result<List<StampCard>> CreateNewAccountingYear(int accountingYear)
    {
        var stampCards = new List<StampCard>();
        foreach (var player in Players)
        {
            var result = AddStampCard(player.Id, (short)accountingYear);
            if (result.IsSuccess)
            {
                stampCards.Add(result.Value);
            }
        }

        return Result.Ok(stampCards);
    }

    /// <summary>
    /// Returns all incomplete stamp cards. A stamp card is expected to be incomplete when the actual number of stamps
    /// is less than the required number of stamps. 
    /// </summary>
    /// <param name="accountingYear">The accounting year.</param>
    /// <param name="numberOfRequiredStamps">The number of required stamps.</param>
    /// <returns>List of incomplete stamp cards.</returns>
    /// <remarks>Since players can be deleted retrospectively, it is not always possible to tell whether a stamp card
    /// is missing for a player. The number of existing stamp cards is
    /// therefore used as the basis for incomplete stamp cards.</remarks>
    public Result<List<StampCard>> GetIncompleteStampCards(int accountingYear, int numberOfRequiredStamps)
    {
        return Result.Ok(Cards.Where(card => card.AccountingYear == accountingYear && card.Stamps.Count < numberOfRequiredStamps)
            .ToList());
    }
    
    /// <summary>
    /// Returns all completed stamp cards. A stamp card is expected to be completed when the actual number of stamps
    /// is equal to the required number of stamps. 
    /// </summary>
    /// <param name="accountingYear">The accounting year.</param>
    /// <param name="numberOfRequiredStamps">The number of required stamps.</param>
    /// <returns>List of completed stamp cards.</returns>
    public Result<List<StampCard>> GetCompleteStampCards(int accountingYear, int numberOfRequiredStamps)
    {
        return Result.Ok(Cards.Where(card => card.AccountingYear == accountingYear && card.Stamps.Count >= numberOfRequiredStamps)
            .ToList());
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