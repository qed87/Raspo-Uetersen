namespace Raspo_Stempelkarten_Backend.Model;

/// <summary>
/// A stamp on a stamp card.
/// </summary>
public class Stamp(Guid id, string reason, string issuer, DateTimeOffset issuedOn)
{
    public Stamp(string reason) 
        : this(Guid.NewGuid(), reason, "dbo", DateTimeOffset.UtcNow)
    {
    }

    public Guid Id { get; set; } = id;
    
    public string Reason { get; set; } = reason;

    public string Issuer { get; set; } = issuer;

    public DateTimeOffset IssuedOn { get; set; } = issuedOn;
    
}