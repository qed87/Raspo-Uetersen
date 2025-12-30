using FluentResults;
using Raspo_Stempelkarten_Backend.Model.Data;

namespace Raspo_Stempelkarten_Backend.Model;

public sealed class StampCardAggregate : IStampCardAggregate
{
    public StampCardAggregate(
        string season,
        string team,
        StampCardLoadContext loadContext)
    {
        Season = season;
        Team = team;
        foreach (var stampCardData in loadContext.StampCards)
        {
            var stampData = loadContext.Stamps.Where(data => data.StampCardId == stampCardData.Id).ToList();
            _stampCards.Add(stampCardData.Id, new StampCard(stampCardData, stampData));
        }
    }
    
    private readonly Dictionary<Guid, StampCard> _stampCards = new();
    

    public  string Team { get; }

    public string Season { get; }

    public ulong? ConcurrencyToken { get; set; }
    
    public Task<IEnumerable<StampCard>> List()
    {
        return Task.FromResult<IEnumerable<StampCard>>(_stampCards.Values.ToList());
    }

    public Task<Stamp?> GetStampById(Guid stampCardId, Guid id)
    {
        var stampCard = _stampCards.Values.SingleOrDefault(stampCard => stampCard.Id == stampCardId);
        if (stampCard is null) return Task.FromResult((Stamp?)null);
        return Task.FromResult(stampCard.GetStamps().SingleOrDefault(stamp => stamp.Id == id));
    }

    public Task<IEnumerable<Stamp>> GetStamps(Guid id)
    {
        var stampCard = _stampCards.Values.SingleOrDefault(stampCard => stampCard.Id == id);
        if (stampCard is null) return Task.FromResult(Enumerable.Empty<Stamp>());
        return Task.FromResult(stampCard.GetStamps());
    }

    public Task<Result<StampCard>> Update(
        Guid id, 
        string recipient, 
        string issuer, 
        int minStamps, 
        int maxStamps, 
        string[] owners)
    {
        var stampCard = _stampCards.Values.SingleOrDefault(stampCard => stampCard.Id == id);
        if (stampCard is null) 
            return Task.FromResult(
                Result.Fail<StampCard>($"Stempelkarte '{id}' konnte nicht gefunden!"));
        stampCard.Update(recipient, issuer, minStamps, maxStamps, owners);
        return Task.FromResult(Result.Ok(stampCard));
    }

    public Task<IEnumerable<StampCard>> GetStampCards() 
        => Task.FromResult<IEnumerable<StampCard>>(_stampCards.Values.ToList());
   
    public Task<Result<StampCard>> AddStampCard(
        string recipient, 
        string issuedBy,
        int minStamps, 
        int maxStamps,
        string[] additionalOwners)
    {
        if (_stampCards.Values.Any(stempelkarte => stempelkarte.Recipient == recipient))
        {
            return Task.FromResult(Result.Fail<StampCard>($"Es liegt bereits eine Stempelkarte für den Empfänger '{recipient}' vor!"));
        }
        
        var stempelkarte = new StampCard(
            Guid.NewGuid(),  
            recipient, 
            issuedBy, 
            maxStamps,
            minStamps);
        
        _stampCards.Add(stempelkarte.Id, stempelkarte);
        SetStampCardOwners(stempelkarte.Id, additionalOwners, issuedBy);
        return Task.FromResult(Result.Ok(stempelkarte));
    }

    public Task<Result<StampCard>> RemoveStampCard(Guid id, string issuedBy)
    {
        if (!_stampCards.TryGetValue(id, out var stempelkarte))
        {
            return Task.FromResult(Result.Fail<StampCard>($"Stempelkarte mit Id='{id}' wurde nicht gefunden!"));
        }
        
        if (!stempelkarte.GetOwners().Contains(issuedBy))
        {
            return Task.FromResult(Result.Fail<StampCard>("Stempelkarten können nur von Besitzern gelöscht werden."));
        }

        if (!_stampCards.Remove(id))
        {
            return Task.FromResult(Result.Fail<StampCard>("Problem beim Löschen."));
        }
        
        return Task.FromResult(Result.Ok(stempelkarte));
    }

    public Task<Result<(IEnumerable<string> AddedOwners, IEnumerable<string> RemovedOwners)>> SetStampCardOwners(
        Guid stampCardId, 
        string[] owners, 
        string issuedBy)
    {
        if (!_stampCards.TryGetValue(stampCardId, out var stempelkarte))
        {
            return Task.FromResult(Result.Fail<(IEnumerable<string>, IEnumerable<string>)>($"Stempelkarte mit Id='{stampCardId}' wurde nicht gefunden!"));
        }

        return Task.FromResult(stempelkarte.SetOwners(issuedBy, owners));
    }
    
    public Task<Result<Stamp>> Stamp(Guid stampCardId, string issuedBy, string? reason)
    {
        if (!_stampCards.TryGetValue(stampCardId, out var stempelkarte))
        {
            return Task.FromResult(Result.Fail<Stamp>($"Stempelkarte mit Id='{stampCardId}' wurde nicht gefunden!"));
        }
        
        var stampResult = stempelkarte.Stamp(issuedBy, reason);
        if (stampResult.IsFailed) return Task.FromResult(stampResult);
        return Task.FromResult(Result.Ok(stampResult.Value));
    }
    
    public Task<Result<Stamp>> EraseStamp(Guid stampCardId, Guid stampId, string issuedBy)
    {
        if (!_stampCards.TryGetValue(stampCardId, out var stempelkarte))
        {
            return Task.FromResult(Result.Fail<Stamp>($"Stempelkarte mit Id='{stampCardId}' wurde nicht gefunden!"));
        }
        
        var stampResult = stempelkarte.EraseStamp(stampId, issuedBy);
        if (stampResult.IsFailed) return Task.FromResult(stampResult);
        return Task.FromResult(Result.Ok(stampResult.Value));
    }

    public Task<StampCard?> GetById(Guid id)
    {
        return Task.FromResult(_stampCards.GetValueOrDefault(id));
    }
}