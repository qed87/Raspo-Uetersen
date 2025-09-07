using FluentResults;
using Raspo_Stempelkarten_Backend.Model.Data;

namespace Raspo_Stempelkarten_Backend.Model;

public class StampCard
{
    private readonly StampCardData _data;
    private readonly List<Stamp> _stamps = [];
    
    public StampCard(
        Guid id,
        string recipient,
        string issuedBy,
        int maxStamps,
        int minStamps,
        string[]? owners = null)
    : this(
        new StampCardData
        {
            Id = id, 
            IssuedBy = issuedBy,
            Recipient = recipient,
            MaxStamps = maxStamps,
            MinStamps = minStamps,
            Owners = [..owners ?? Enumerable.Empty<string>(), issuedBy]
        }, [])
    {
    }

    internal StampCard(
        StampCardData stampCardData, 
        IEnumerable<StampData> stampDataList)
    {
        _data = stampCardData;
        foreach (var stampData in stampDataList)
        {
            _stamps.Add(new Stamp(stampData));
        }
    }

    public Guid Id => _data.Id; 
    
    public string Recipient 
    {
        get => _data.Recipient;
        set
        {
            if (value == _data.Recipient) return;
            _data.Recipient = value;
        }
    }

    public string IssuedBy => _data.IssuedBy;

    public int MaxStamps
    {
        get => _data.MaxStamps;
        set
        {
            if (value == _data.MaxStamps) return;
            _data.MaxStamps = value;
        }
    }

    public int MinStamps
    {
        get => _data.MinStamps;
        set
        {
            if (value == _data.MinStamps) return;
            _data.MinStamps = value;
        }
    }

    public Result<(IEnumerable<string> AddedOwners, IEnumerable<string> RemovedOwners)> SetOwners(string issuedBy, string[] owners)
    {
        if (_data.Owners.Contains(issuedBy)) return Result.Fail($"Besitzer '{issuedBy}' ist bereits vorhanden!");
        if (issuedBy != IssuedBy && !owners.Contains(IssuedBy)) 
            return Result.Fail($"Nur der Ersteller '{IssuedBy}' kann sich als Besitzer entfernen!");
        var removedOwners = _data.Owners.Except(owners);
        var addedOwners = owners.Except(_data.Owners);
        return Result.Ok((addedOwners, removedOwners));
    }

    public Result<Stamp> EraseStamp(Guid id, string issuedBy)
    {
        var stamp = _stamps.FirstOrDefault(stamp => stamp.Id == id);
        if (stamp is null) return Result.Fail($"Stempel mit Id='{id}' konnte nicht gefunden werden!");
        if (!_data.Owners.Contains(issuedBy)) return Result.Fail("Nur Besitzer können Stempel entfernen!");
        _stamps.Remove(stamp);
        return Result.Ok(stamp);
    }

    public IEnumerable<string> GetOwners() => _data.Owners.Distinct().ToList();

    public Result<Stamp> Stamp(string issuedBy, string? reason)
    {
        if (!_data.Owners.Contains(issuedBy))
        {
            return Result.Fail("Stempelkarten können nur von Besitzern gestempelt werden.");
        }

        var stamp = new Stamp(Guid.NewGuid(), issuedBy, reason);
        _stamps.Add(stamp);

        return Result.Ok(stamp);
    }

    public IEnumerable<Stamp> GetStamps() => _stamps.ToList();
}