using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FluentResults;
using Raspo_Stempelkarten_Backend.Model.Data;

namespace Raspo_Stempelkarten_Backend.Model;

public class StampCard : INotifyPropertyChanged
{
    private readonly StampCardData _data;
    public ObservableCollection<Stamp> Stamps { get; } = [];
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
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
            CreatedBy = issuedBy,
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
        _data.PropertyChanged += DataOnPropertyChanged;
        foreach (var stampData in stampDataList)
        {
            Stamps.Add(new Stamp(stampData));
        }
    }
    
    private void DataOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(e.PropertyName);
    }

    public Guid Id => _data.Id; 
    
    public string Recipient 
    {
        get => _data.Recipient;
        set => _data.Recipient = value;
    }

    public string IssuedBy => _data.CreatedBy;

    public int MaxStamps
    {
        get => _data.MaxStamps;
        set => _data.MaxStamps = value;
    }

    public int MinStamps
    {
        get => _data.MinStamps;
        set => _data.MinStamps = value;
    }

    public Result<(IEnumerable<string> AddedOwners, IEnumerable<string> RemovedOwners)> SetOwners(
        string issuedBy, 
        string[] owners)
    {
        var existingOwners = new HashSet<string>(_data.Owners);
        var newOwners = new HashSet<string>(owners) { issuedBy };
        var removedOwners = existingOwners.Except(owners).ToList();
        var addedOwners = newOwners.Except(existingOwners).ToList();
        if (issuedBy != IssuedBy && removedOwners.Contains(IssuedBy))
            return Result.Fail($"Nur der Eigentümer'{IssuedBy}' kann sich als Besitzer entfernen!");
        foreach (var removedOwner in removedOwners)
            _data.Owners.Remove(removedOwner);
        foreach (var addedOwner in addedOwners)
            _data.Owners.Add(addedOwner);
        return Result.Ok<(IEnumerable<string>, IEnumerable<string>)>((addedOwners, removedOwners));
    }
    
    
    public void Update(string recipient, string issuer, int minStamps, int maxStamps, string[] owners)
    {
        Recipient = recipient;
        //IssuedBy = issuer;
        MaxStamps = maxStamps;
        MinStamps = minStamps;
    }

    public Result<Stamp> EraseStamp(Guid id, string issuedBy)
    {
        var stamp = Stamps.FirstOrDefault(stamp => stamp.Id == id);
        if (stamp is null) return Result.Fail($"Stempel mit Id='{id}' konnte nicht gefunden werden!");
        if (!_data.Owners.Contains(issuedBy)) return Result.Fail("Nur Besitzer können Stempel entfernen!");
        Stamps.Remove(stamp);
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
        Stamps.Add(stamp);

        return Result.Ok(stamp);
    }

    public IEnumerable<Stamp> GetStamps() => Stamps.ToList();
}