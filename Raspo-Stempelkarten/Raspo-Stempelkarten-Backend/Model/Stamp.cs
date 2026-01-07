namespace Raspo_Stempelkarten_Backend.Model;

public class Stamp(Guid id, string reason, string issuedBy, DateTimeOffset issuedAt)
{
    public Stamp(string reason) 
        : this(Guid.NewGuid(), reason, "dbo", DateTimeOffset.UtcNow)
    {
    }

    public Guid Id { get; set; } = id;
    
    public string Reason { get; set; } = reason;

    public string IssuedBy { get; set; } = issuedBy;

    public DateTimeOffset IssuedAt { get; set; } = issuedAt;
    
}