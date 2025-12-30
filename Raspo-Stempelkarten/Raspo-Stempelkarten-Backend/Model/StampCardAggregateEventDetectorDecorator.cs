using DispatchR;
using FluentResults;
using JetBrains.Annotations;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Model;

[UsedImplicitly]
public class StampCardAggregateEventDetectorDecorator(
    string season,
    string team,
    IStampCardAggregate inner,
    IMediator mediator) 
    : IStampCardAggregate
{
    public Task<IEnumerable<StampCard>> GetStampCards()
    {
        return inner.GetStampCards();
    }

    public async Task<Result<StampCard>> AddStampCard(
        string recipient, 
        string issuedBy, 
        int minStamps, 
        int maxStamps, 
        string[] additionalOwners)
    {
        var result = await inner.AddStampCard(recipient, issuedBy, minStamps, maxStamps, additionalOwners);
        if (result.IsFailed) return result;
        await mediator.Publish(
            new StampCardCreated
            {
                Id = result.Value.Id,
                Team = team,
                Season = season,
                Recipient = result.Value.Recipient,
                IssuedBy = result.Value.IssuedBy,
                MinStamps = result.Value.MinStamps, 
                MaxStamps = result.Value.MaxStamps, 
            }, 
            CancellationToken.None);
        foreach (var owner in result.Value.GetOwners())
        {
            await mediator.Publish(
                new StampCardOwnerAdded
                {
                    StampCardId = result.Value.Id,
                    Name = owner
                }, 
                CancellationToken.None);
        }
        
        return result;
    }

    public async Task<Result<StampCard>> RemoveStampCard(Guid id, string issuedBy)
    {
        var result = await inner.RemoveStampCard(id,  issuedBy);
        if (result.IsFailed) return result;
        await mediator.Publish(
            new StampCardDeleted
            {
                Id = result.Value.Id,
                IssuedBy = result.Value.IssuedBy
            }, 
            CancellationToken.None);
        return result;
    }

    public async Task<Result<(IEnumerable<string> AddedOwners, IEnumerable<string> RemovedOwners)>> SetStampCardOwners(Guid stampCardId, string[] owners, string issuedBy)
    {
        var result = await inner.SetStampCardOwners(stampCardId,  owners, issuedBy);
        if (result.IsFailed) return result;
        foreach (var addedOwner in result.Value.AddedOwners)
        {
            await mediator.Publish(
                new StampCardOwnerAdded
                {
                    Name = addedOwner,
                    StampCardId = stampCardId
                }, 
                CancellationToken.None);    
        }
        
        foreach (var removedOwner in result.Value.RemovedOwners)
        {
            await mediator.Publish(
                new StampCardOwnerAdded
                {
                    Name = removedOwner,
                    StampCardId = stampCardId
                }, 
                CancellationToken.None);    
        }
        
        return result;
    }

    public async Task<Result<Stamp>> Stamp(Guid stampCardId, string issuedBy, string? reason)
    {
        var result = await inner.Stamp(stampCardId, issuedBy, reason);
        if (result.IsFailed) return result;
        await mediator.Publish(
            new StampCardStamped
            {
                Id = result.Value.Id,
                IssuedBy = result.Value.IssuedBy,
                Reason = result.Value.Reason,
                StampCardId = stampCardId
            }, 
            CancellationToken.None);
        return result;
    }

    public async Task<Result<Stamp>> EraseStamp(Guid stampCardId, Guid stampId, string issuedBy)
    {
        var result = await inner.EraseStamp(stampCardId, stampId, issuedBy);
        if (result.IsFailed) return result;
        await mediator.Publish(
            new StampCardStampErased
            {
                Id = result.Value.Id,
                StampCardId = stampCardId,
                IssuedBy = result.Value.IssuedBy
            },
            CancellationToken.None);
        return result;
    }

    public Task<StampCard?> GetById(Guid id)
    {
        return inner.GetById(id);
    }
    
    public Task<IEnumerable<StampCard>> List()
    {
        return inner.List();
    }

    public Task<Stamp?> GetStampById(Guid stampCardId, Guid id)
    {
        return inner.GetStampById(stampCardId, id);
    }

    public Task<IEnumerable<Stamp>> GetStamps(Guid id)
    {
        return inner.GetStamps(id);
    }

    public Task<Result<StampCard>> Update(
        Guid id, 
        string recipient, 
        string issuer, 
        int minStamps, 
        int maxStamps, 
        string[] owners)
    {
        throw new NotImplementedException();
    }

    public ulong? ConcurrencyToken => inner.ConcurrencyToken;
   
}