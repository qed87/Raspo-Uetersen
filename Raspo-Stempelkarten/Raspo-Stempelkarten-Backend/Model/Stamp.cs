using Raspo_Stempelkarten_Backend.Events;

namespace Raspo_Stempelkarten_Backend.Model;

public class Stamp(Guid id, string issuedBy, string reason)
{
    public Guid Id { get; set; } = id;
    
    public string IssuedBy { get; set; } = issuedBy;
    
    public string Reason { get; } = reason;

    public IEnumerable<UserEvent> GetChanges()
    {
        return [];
    }
}