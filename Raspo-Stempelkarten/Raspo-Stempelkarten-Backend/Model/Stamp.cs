using Raspo_Stempelkarten_Backend.Model.Data;

namespace Raspo_Stempelkarten_Backend.Model;

public sealed class Stamp(StampData data)
{
    public Stamp(Guid id, string issuedBy, string? reason)
    : this(new StampData { Id = id, IssuedBy = issuedBy, Reason = reason })
    {
    }

    public Guid Id => data.Id;
    
    public string IssuedBy => data.IssuedBy;

    public string? Reason => data.Reason;
}