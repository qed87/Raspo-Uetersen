using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Model;

public class Stempelkarte(
    Guid id, 
    string recipient, 
    string owner,
    int maxStamps, 
    int minStamps,
    string[]? additionalOwners = null)
{
    private StampCardUpdated? _cardUpdated;
    private List<UserEvent> _changes = [];
    private readonly List<Stamp> _stamps = [];
    private bool _isLoaded;

    public Guid Id { get; } = id;
    
    public string Recipient { get; set; } = recipient;
    
    public string Owner { get; } = owner;
    
    public List<string> AdditionalOwners { get; } = additionalOwners?.ToList() ?? [];
    
    public IEnumerable<Stamp> Stamps => _stamps;

    private int _maxStamps = maxStamps;
    public int MaxStamps
    {
        get => _maxStamps;
        set
        {
            _maxStamps = value;
            if (!_isLoaded)
            {
                return;
            }
            
            _cardUpdated ??= new StampCardUpdated { Team = "TBD", Season = "TBD", Recipient = Recipient, MinStamps = MinStamps, AdditionalOwners = AdditionalOwners.ToArray() };
            _cardUpdated.MaxStamps = _maxStamps;
        }
    }
    
    private int _minStamps = minStamps;
    public int MinStamps
    {
        get => _minStamps;
        set => _minStamps = value;
    }

    public void Stamp(Guid id, string issuedBy, string reason)
    {
        _stamps.Add(new Stamp(id, issuedBy, reason));
    }

    public void SetLoaded()
    {
        _isLoaded = true;
    }

    public IEnumerable<UserEvent> GetChanges()
    {
        var changes = _changes.ToList(); 
        foreach (var stamp in _stamps)
        {
            changes.AddRange(stamp.GetChanges());
        }

        return changes;
    }

    public void Update(string[] additionalOwners)
    {
        throw new NotImplementedException();
    }
}