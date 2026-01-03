using FluentResults;

namespace Raspo_Stempelkarten_Backend.Model;

public class StampModel : IStampModel
{
    public ulong? Version { get; set; }

    public List<Player> Players { get; } = [];
    
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
}