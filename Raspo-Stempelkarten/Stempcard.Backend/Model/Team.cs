using System.Security.Claims;
using System.Security.Principal;
using DispatchR;
using FluentResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Model;

/// <summary>
/// The team model.
/// </summary>
public class Team(IMediator mediator, IPrincipal userPrincipal) : ITeamAggregate
{
    /// <inheritdoc />
    public bool Deleted { get; set; }
    
    /// <inheritdoc />
    public ulong? Version { get; set; }

    /// <inheritdoc />
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc />
    public List<Member> Members { get; } = [];
    
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
    public Task<Result> UpdateAsync(string name)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> AddMemberAsync(string firstName, string surname, DateOnly birthdate, string birthplace)
    {
        if (string.IsNullOrEmpty(firstName)) return Result.Fail("Vorname is leer");
        if (string.IsNullOrEmpty(surname)) return Result.Fail("Nachname is leer");
        if (birthdate > DateOnly.FromDateTime(DateTime.Now)) return Result.Fail("Date of birth must not be in the future");
        var newMember = new Member(Guid.NewGuid(), firstName, surname, birthdate, birthplace);
        var memberFound = Members.SingleOrDefault(member => member.Equals(newMember));
        if (memberFound is not null && memberFound.Deleted)
        {
            // reactivate player
            memberFound.Deleted = false;
            await mediator.Publish(
                new MemberAdded(memberFound.Id, memberFound.FirstName, memberFound.LastName, 
                    memberFound.Birthdate, memberFound.Birthplace, 
                    GetUserName(), DateTimeOffset.UtcNow), 
                CancellationToken.None);
            return Result.Ok(memberFound.Id);
        }
        
        if (memberFound is not null && !memberFound.Deleted)
        {
            return Result.Fail("Player already exist");
        }
        
        Members.Add(newMember);
        await mediator.Publish(
            new MemberAdded(newMember.Id, newMember.FirstName, newMember.LastName, 
                newMember.Birthdate, newMember.Birthplace, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok(newMember.Id);
    }

    /// <inheritdoc />
    public async Task<Result> AddCoachAsync(string email)
    {
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
        var coachFound = Coaches.SingleOrDefault(coach => coach.Email == email);
        if (coachFound is null) return Result.Fail("Coach existiert nicht.");
        Coaches.Remove(coachFound);
        await mediator.Publish(
            new CoachRemoved(email, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok();
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> RemoveMemberAsync(Guid memberId)
    {
        var memberFound = Members.SingleOrDefault(player => player.Id.Equals(memberId));
        if (memberFound is null) return Result.Fail("Member nicht gefunden.");
        memberFound.Deleted = true;
        await mediator.Publish(
            new MemberRemoved(memberFound.Id, GetUserName(), DateTimeOffset.UtcNow), 
                CancellationToken.None);
        return Result.Ok(memberFound.Id);
    }

    /// <inheritdoc />
    public async Task<Result<Guid>> AddStampCardAsync(Guid memberId, short accountingYear)
    {
        var memberResponse = GetActivePlayerById(memberId);
        if(!memberResponse.IsSuccess) return memberResponse.ToResult();
        var newStampCard = new StampCard(memberResponse.Value.Id, accountingYear, GetUserName(), DateTimeOffset.UtcNow);
        var cardFound = Cards.SingleOrDefault(stampCard => stampCard.Equals(newStampCard));
        if (cardFound is not null) return Result.Fail("Stempelkarte existiert bereits.");
        Cards.Add(newStampCard);
        await mediator.Publish(
            new StampCardAdded(newStampCard.Id, newStampCard.AccountingYear, newStampCard.MemberId, 
                newStampCard.Issuer, newStampCard.IssuedOn), 
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
            new StampAdded(newStamp.Id, stampCard.Id, newStamp.Reason, newStamp.Issuer, newStamp.IssuedOn), 
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
            new StampErased(stamp.Id, stampCardId, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok(stampId);
    }

    /// <inheritdoc />
    public async Task<Result> CreateNewAccountingYearAsync(int accountingYear)
    {
        var response = Result.Ok();
        foreach (var player in Members)
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
            new TeamDeleted(Id, GetUserName(), DateTimeOffset.UtcNow), 
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
            new StampCardRemoved(id, GetUserName(), DateTimeOffset.UtcNow), 
            CancellationToken.None);
        return Result.Ok(id);
    }

    private Result<Member> GetActivePlayerById(Guid memberId)
    {
        var memberFound = Members.SingleOrDefault(member => member.Id.Equals(memberId) && !member.Deleted);
        return memberFound is null 
            ? Result.Fail($"No player found with Id = {memberId}") 
            : Result.Ok(memberFound);
    }

    private string GetUserName()
    {
        return userPrincipal.Identity?.Name ?? string.Empty;
    }
}