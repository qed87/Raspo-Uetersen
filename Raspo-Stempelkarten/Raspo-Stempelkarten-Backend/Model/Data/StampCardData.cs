using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Raspo_Stempelkarten_Backend.Model.Data;

public class StampCardData : INotifyPropertyChanged
{
    private Guid _id;
    private DateTimeOffset _created;
    private string _createdBy = null!;
    private DateTimeOffset? _updated;
    private string? _lastUpdatedBy;
    private string _team;
    private string _season;
    private string _recipient = null!;
    private int _maxStamps;
    private int _minStamps;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public Guid Id
    {
        get => _id;
        init => SetField(ref _id, value);
    }

    public DateTimeOffset Created
    {
        get => _created;
        set => SetField(ref _created, value);
    }

    public string CreatedBy
    {
        get => _createdBy;
        set => SetField(ref _createdBy, value);
    }

    public DateTimeOffset? Updated
    {
        get => _updated;
        set => SetField(ref _updated, value);
    }

    public string? LastUpdatedBy
    {
        get => _lastUpdatedBy;
        set => SetField(ref _lastUpdatedBy, value);
    }

    public string Team
    {
        get => _team;
        set => SetField(ref _team, value);
    }

    public string Season
    {
        get => _season;
        set => SetField(ref _season, value);
    }

    public string Recipient
    {
        get => _recipient;
        set => SetField(ref _recipient, value);
    }

    public int MaxStamps
    {
        get => _maxStamps;
        set => SetField(ref _maxStamps, value);
    }

    public int MinStamps
    {
        get => _minStamps;
        set => SetField(ref _minStamps, value);
    }

    public ObservableCollection<string> Owners { get; set; } = [];
}