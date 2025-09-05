using FluentResults;
using JetBrains.Annotations;
using LiteBus.Events.Abstractions;
using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Model;

[UsedImplicitly]
public class StampCardAggregateEventDetectorDecorator(IStampCardAggregate inner, IEventMediator eventMediator) 
    : IStampCardAggregate
{
    public Task<IEnumerable<StampCard>> GetStampCards()
    {
        return inner.GetStampCards();
    }

    public async Task<Result<StampCard>> AddStampCard(string recipient, string issuedBy, int minStamps, int maxStamps)
    {
        var result = await inner.AddStampCard(recipient, issuedBy, minStamps, maxStamps);
        if (result.IsFailed) return result;
        await eventMediator.PublishAsync(
            new StampCardCreated
            {
                Id = result.Value.Id,
                IssuedBy = result.Value.IssuedBy,
                MinStamps = result.Value.MinStamps, 
                MaxStamps = result.Value.MaxStamps, 
            });
        return result;
    }

    public async Task<Result<StampCard>> RemoveStampCard(Guid id, string issuedBy)
    {
        var result = await inner.RemoveStampCard(id,  issuedBy);
        if (result.IsFailed) return result;
        await eventMediator.PublishAsync(
            new StampCardDeleted
            {
                Id = result.Value.Id,
                IssuedBy = result.Value.IssuedBy
            });
        return result;
    }

    public async Task<Result<(IEnumerable<string> AddedOwners, IEnumerable<string> RemovedOwners)>> SetStampCardOwners(Guid stampCardId, string[] owners, string issuedBy)
    {
        var result = await inner.SetStampCardOwners(stampCardId,  owners, issuedBy);
        if (result.IsFailed) return result;
        foreach (var addedOwner in result.Value.AddedOwners)
        {
            await eventMediator.PublishAsync(
                new StampCardOwnerAdded
                {
                    Name = addedOwner,
                    StampCardId = stampCardId
                });    
        }
        
        foreach (var removedOwner in result.Value.RemovedOwners)
        {
            await eventMediator.PublishAsync(
                new StampCardOwnerAdded
                {
                    Name = removedOwner,
                    StampCardId = stampCardId
                });    
        }
        
        return result;
    }

    public async Task<Result<Stamp>> Stamp(Guid stampCardId, string issuedBy, string? reason)
    {
        var result = await inner.Stamp(stampCardId, issuedBy, reason);
        if (result.IsFailed) return result;
        await eventMediator.PublishAsync(
            new StampCardStamped
            {
                Id = result.Value.Id,
                IssuedBy = result.Value.IssuedBy,
                Reason = result.Value.Reason,
                StampCardId = stampCardId
            });
        return result;
    }

    public async Task<Result<Stamp>> EraseStamp(Guid stampCardId, Guid stampId, string issuedBy)
    {
        var result = await inner.EraseStamp(stampCardId, stampId, issuedBy);
        if (result.IsFailed) return result;
        await eventMediator.PublishAsync(
            new StampCardStampErased
            {
                Id = result.Value.Id,
                StampCardId = stampCardId,
                IssuedBy = result.Value.IssuedBy
            });
        return result;
    }

    public Task<StampCard?> GetById(Guid id)
    {
        return inner.GetById(id);
    }

    public ulong? ConcurrencyToken => inner.ConcurrencyToken;
}