using DispatchR;
using FluentResults;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Model;

/// <summary>
/// The team model.
/// </summary>
public class Team(IMediator mediator) : ITeamAggregate
{
    /// <inheritdoc />
    public bool Deleted { get; set; }
    
    /// <inheritdoc />
    public ulong? Version { get; set; }

    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public List<Player> Players { get; } = [];
    
    /// <inheritdoc />
    public List<StampCard> Cards { get; } = [];

    /// <inheritdoc />
    public List<Coach> Coaches { get; } = [];

    /// <inheritdoc />
    public async Task<Result<Guid>> AddPlayerAsync(string firstName, string surname, DateOnly birthdate, string birthplace)
    {
        if(string.IsNullOrEmpty(firstName)) return Result.Fail("Vorname is leer");
        if(string.IsNullOrEmpty(surname)) return Result.Fail("Nachname is leer");
        if(birthdate > DateOnly.FromDateTime(DateTime.Now)) return Result.Fail("Date of birth must not be in the future");
        var newPlayer = new Player(Guid.NewGuid(), firstName, surname, birthdate, birthplace);
        var playerFound = Players.SingleOrDefault(player => player.Equals(newPlayer));
        if (playerFound is not null && playerFound.Deleted)
        {
            // reactivate player
            playerFound.Deleted = false;
            await mediator.Publish(
                new PlayerAdded(playerFound.Id, playerFound.FirstName, playerFound.LastName, playerFound.Birthdate, playerFound.Birthplace), 
                CancellationToken.None);
            return Result.Ok(playerFound.Id);
        }
        
        if (playerFound is not null && !playerFound.Deleted)
        {
            return Result.Fail("Player already exist");
        }
        
        Players.Add(newPlayer);
        await mediator.Publish(
            new PlayerAdded(newPlayer.Id, newPlayer.FirstName, newPlayer.LastName, newPlayer.Birthdate, newPlayer.Birthplace), 
            CancellationToken.None);
        return Result.Ok(newPlayer.Id);
    }

    /// <summary>
    /// Adds a new coach to the team.
    /// </summary>
    /// <param name="email">The email of the coach.</param>
    /// <param name="issuedBy">The name of the issuer.</param>
    /// <returns></returns>
    public async Task<Result> AddCoach(string email, string issuedBy)
    {
        if (Coaches.Any(coach => coach.Email == email)) return Result.Fail("Coach existiert bereits.");
        Coaches.Add(new Coach { Email = email, IssuedBy = issuedBy, IssuedDate = DateTimeOffset.UtcNow });
        await mediator.Publish(
            new CoachAdded(email, issuedBy, DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok();
    }
    
    /// <summary>
    /// Removes a coach to the team.
    /// </summary>
    /// <param name="email">The email of the coach.</param>
    /// <param name="issuedBy">The name of the issuer.</param>
    /// <returns></returns>
    public async Task<Result> RemoveCoach(string email, string issuedBy)
    {
        var coachFound = Coaches.SingleOrDefault(coach => coach.Email == email);
        if (coachFound is null) return Result.Fail("Coach existiert nicht.");
        Coaches.Remove(coachFound);
        await mediator.Publish(
            new CoachRemoved(email, issuedBy, DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok();
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> DeletePlayerAsync(Guid playerId)
    {
        var playerFound = Players.SingleOrDefault(player => player.Id.Equals(playerId));
        if (playerFound is null) return Result.Fail("Player not found");
        playerFound.Deleted = true;
        await mediator.Publish(
            new PlayerDeleted(playerFound.Id), 
            CancellationToken.None);
        return Result.Ok(playerFound.Id);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> AddStampCardAsync(Guid playerId, short accountingYear)
    {
        var playerResponse = GetActivePlayerById(playerId);
        if(!playerResponse.IsSuccess) return playerResponse.ToResult();
        var newStampCard = new StampCard(playerResponse.Value.Id, accountingYear);
        var cardFound = Cards.SingleOrDefault(stampCard => stampCard.Equals(newStampCard));
        if (cardFound is not null) return Result.Fail("Stempelkarte existiert bereits.");
        Cards.Add(newStampCard);
        await mediator.Publish(
            new StampCardAdded(newStampCard.Id, newStampCard.AccountingYear, newStampCard.PlayerId, newStampCard.IssuedDate), 
            CancellationToken.None);
        return Result.Ok(newStampCard.Id);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> StampStampCardAsync(Guid stampCardId, string reason)
    {
        var stampCard = Cards.SingleOrDefault(card => card.Id == stampCardId);
        if(stampCard is null) return Result.Fail("Stempelkarte nicht gefunden.");
        var newStamp = new Stamp(reason);
        stampCard.Stamps.Add(newStamp);
        await mediator.Publish(
            new StampAdded(newStamp.Id, stampCard.Id, newStamp.Reason, newStamp.IssuedBy, newStamp.IssuedDate), 
            CancellationToken.None);
        return Result.Ok(newStamp.Id);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> EraseStampAsync(Guid stampCardId, Guid stampId)
    {
        var stampCard = Cards.SingleOrDefault(card => card.Id == stampCardId);
        if(stampCard is null) return Result.Fail("Stempelkarte nicht gefunden.");
        var stamp = stampCard.Stamps.SingleOrDefault(stamp => stamp.Id == stampId);
        if(stamp is null) return Result.Fail("Stempel nicht gefunden.");
        stampCard.Stamps.Remove(stamp);
        await mediator.Publish(
            new StampErased(stamp.Id, stampCardId), 
            CancellationToken.None);
        return Result.Ok(stampId);
    }

    /// <inheritdoc />
    public async Task<Result> CreateNewAccountingYearAsync(int accountingYear)
    {
        var response = Result.Ok();
        foreach (var player in Players)
        {
            var result = await AddStampCardAsync(player.Id, (short)accountingYear);
            if (result.IsFailed)
            {
                response = Result.Merge(response, result.ToResult());
            }
        }

        return response;
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

    /// <inheritdoc />
    public async Task<Result<string>> DeleteTeamAsync()
    {
        Deleted = true;
        await mediator.Publish(
            new TeamDeleted(Id), 
            CancellationToken.None);
        return Id;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> DeleteStampCard(Guid id)
    {
        var stampCard = Cards.SingleOrDefault(card => card.Id == id);
        if(stampCard is null) return Result.Fail("StampCard not found");
        Cards.Remove(stampCard);
        await mediator.Publish(
            new StampCardRemoved(id), 
            CancellationToken.None);
        return Result.Ok(id);
    }

    private Result<Player> GetActivePlayerById(Guid playerId)
    {
        var playerFound = Players.SingleOrDefault(player => player.Id.Equals(playerId) && !player.Deleted);
        return playerFound is null 
            ? Result.Fail($"No player found with Id = {playerId}") 
            : Result.Ok(playerFound);
    }
}