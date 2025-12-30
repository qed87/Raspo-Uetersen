using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Raspo_Stempelkarten_Backend.Model.Data;

public class StampData : INotifyPropertyChanged
{
    private Guid _id;
    private Guid _stampCardId;
    private string _issuedBy = null!;
    private string? _reason;
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
        set => SetField(ref _id, value);
    }

    public Guid StampCardId
    {
        get => _stampCardId;
        set => SetField(ref _stampCardId, value);
    }

    public string IssuedBy
    {
        get => _issuedBy;
        set => SetField(ref _issuedBy, value);
    }

    public string? Reason
    {
        get => _reason;
        set => SetField(ref _reason, value);
    }
}