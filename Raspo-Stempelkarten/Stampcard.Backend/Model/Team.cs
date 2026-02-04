using System.Security.Principal;
using DispatchR;
using FluentResults;
using StampCard.Backend.Events;

namespace StampCard.Backend.Model;

/// <summary>
/// The team model.
/// </summary>
public class Team(IMediator mediator, IPrincipal userPrincipal) : ITeamAggregate
{
    private const string TeamAlreadyDeleteMsg = "Team ist bereits gelöscht!"; 
    
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

    /// <summary>
    /// The team name.
    /// </summary>
    public string Name { get; set; } = null!;
    
    /// <summary>
    /// The club name.
    /// </summary>
    public string Club { get; set; } = null!;
    
    /// <summary>
    /// The user that created the team.
    /// </summary>
    public string CreatedBy { get; set; } = null!;
    
    /// <summary>
    /// The timestamp when the team was created. 
    /// </summary>
    public DateTimeOffset CreatedOn { get; set; }
    
    /// <summary>
    /// The last editor of this team. 
    /// </summary>
    public string LastUpdatedBy { get; set; } = null!;
    
    /// <summary>
    /// The last timestamp when the team was updated.
    /// </summary>
    public DateTimeOffset LastModified { get; set; }

    /// <inheritdoc />
    public async Task<Result> UpdateAsync(string name)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        if (string.IsNullOrEmpty(name)) return Result.Fail("Name darf nicht leer sein.");
        Name = name;
        await mediator.Publish(
            new TeamUpdated(Id, name, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok();
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> AddPlayerAsync(string firstName, string surname, DateOnly birthdate, string birthplace)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        if (string.IsNullOrEmpty(firstName)) return Result.Fail("Vorname is leer");
        if (string.IsNullOrEmpty(surname)) return Result.Fail("Nachname is leer");
        if (birthdate > DateOnly.FromDateTime(DateTime.Now)) return Result.Fail("Geburtsdatum darf nicht in der Zukunft liegen.");
        var newPlayer = new Player(Guid.NewGuid(), firstName, surname, birthdate, birthplace);
        var playerFound = Players.SingleOrDefault(player => player.Equals(newPlayer));
        if (playerFound is not null && playerFound.Deleted)
        {
            // reactivate player
            playerFound.Deleted = false;
            await mediator.Publish(
                new PlayerAdded(playerFound.Id, playerFound.FirstName, playerFound.LastName, 
                    playerFound.Birthdate, playerFound.Birthplace, 
                    GetUserName(), DateTimeOffset.UtcNow), 
                CancellationToken.None);
            return Result.Ok(playerFound.Id);
        }
        
        if (playerFound is not null && !playerFound.Deleted)
        {
            return Result.Fail("Spieler existiert bereits.");
        }
        
        Players.Add(newPlayer);
        await mediator.Publish(
            new PlayerAdded(newPlayer.Id, newPlayer.FirstName, newPlayer.LastName, 
                newPlayer.Birthdate, newPlayer.Birthplace, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok(newPlayer.Id);
    }

    /// <inheritdoc />
    public async Task<Result> AddCoachAsync(string email)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        if (Coaches.Any(coach => coach.Email == email)) return Result.Fail("Coach existiert bereits.");
        var newCoach = new Coach { Email = email, Issuer = GetUserName(), IssuedOn = DateTimeOffset.UtcNow };
        Coaches.Add(newCoach);
        await mediator.Publish(
            new CoachAdded(newCoach.Email, newCoach.Issuer, newCoach.IssuedOn), 
            CancellationToken.None);
        return Result.Ok();
    }
    
    /// <inheritdoc />
    public async Task<Result> RemoveCoach(string email)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        var coachFound = Coaches.SingleOrDefault(coach => coach.Email == email);
        if (coachFound is null) return Result.Fail("Coach existiert nicht.");
        Coaches.Remove(coachFound);
        await mediator.Publish(
            new CoachRemoved(email, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok();
    }

    /// <inheritdoc />
    public Task<List<Stamp>> GetStampsFromStampCardAsync(Guid stampCardId)
    {
        if (Deleted) return Task.FromResult<List<Stamp>>([]);
        var card = Cards.SingleOrDefault(card => card.Id == stampCardId);
        if (card is null) return Task.FromResult<List<Stamp>>([]);
        return Task.FromResult(card.Stamps.ToList());
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> RemovePlayerAsync(Guid playerId)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        var playerFound = Players.SingleOrDefault(player => player.Id.Equals(playerId));
        if (playerFound is null) return Result.Fail("Spieler nicht gefunden.");
        playerFound.Deleted = true;
        await mediator.Publish(
            new PlayerRemoved(playerFound.Id, GetUserName(), DateTimeOffset.UtcNow), 
                CancellationToken.None);
        return Result.Ok(playerFound.Id);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> AddStampCardAsync(Guid playerId, short accountingYear)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        var playerResponse = GetActivePlayerById(playerId);
        if(!playerResponse.IsSuccess) return playerResponse.ToResult();
        var newStampCard = new StampCard(playerResponse.Value.Id, accountingYear, GetUserName(), DateTimeOffset.UtcNow);
        var cardFound = Cards.SingleOrDefault(stampCard => stampCard.Equals(newStampCard));
        if (cardFound is not null) return Result.Fail("Stempelkarte existiert bereits.");
        Cards.Add(newStampCard);
        await mediator.Publish(
            new StampCardAdded(newStampCard.Id, newStampCard.AccountingYear, newStampCard.PlayerId, 
                newStampCard.Issuer, newStampCard.IssuedOn), 
            CancellationToken.None);
        return Result.Ok(newStampCard.Id);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> StampStampCardAsync(Guid stampCardId, string reason)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        var stampCard = Cards.SingleOrDefault(card => card.Id == stampCardId);
        if(stampCard is null) return Result.Fail("Stempelkarte nicht gefunden.");
        var newStamp = new Stamp(reason);
        stampCard.Stamps.Add(newStamp);
        await mediator.Publish(
            new StampAdded(newStamp.Id, stampCard.Id, newStamp.Reason, newStamp.Issuer, newStamp.IssuedOn), 
            CancellationToken.None);
        return Result.Ok(newStamp.Id);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> EraseStampAsync(Guid stampCardId, Guid stampId)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        var stampCard = Cards.SingleOrDefault(card => card.Id == stampCardId);
        if(stampCard is null) return Result.Fail("Stempelkarte nicht gefunden.");
        var stamp = stampCard.Stamps.SingleOrDefault(stamp => stamp.Id == stampId);
        if(stamp is null) return Result.Fail("Stempel nicht gefunden.");
        stampCard.Stamps.Remove(stamp);
        await mediator.Publish(
            new StampErased(stamp.Id, stampCardId, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok(stampId);
    }

    /// <inheritdoc />
    public async Task<Result> CreateNewAccountingYearAsync(int accountingYear)
    {
        foreach (var player in Players)
            await AddStampCardAsync(player.Id, (short)accountingYear);

        return Result.Ok();
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
            new TeamDeleted(Id, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Id;
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> DeleteStampCard(Guid id)
    {
        if (Deleted) return Result.Fail(TeamAlreadyDeleteMsg);
        var stampCard = Cards.SingleOrDefault(card => card.Id == id);
        if(stampCard is null) return Result.Fail("StampCard not found");
        Cards.Remove(stampCard);
        await mediator.Publish(
            new StampCardRemoved(id, GetUserName(), DateTimeOffset.UtcNow), 
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

    private string GetUserName()
    {
        return userPrincipal.Identity?.Name ?? string.Empty;
    }
}