namespace Raspo_Stempelkarten_Backend.Model;

public class StampCard(Guid id, Guid issuedTo, DateTimeOffset issuedAt, short accountingYear)
{
    public StampCard(Guid issuedTo, short accountingYear)
        : this(Guid.NewGuid(), issuedTo, DateTimeOffset.UtcNow, accountingYear)
    {
    }

    public Guid Id { get; set; } = id;
    
    public Guid IssuedTo { get; } = issuedTo;
    
    public DateTimeOffset IssuedAt { get; } = issuedAt;

    public short AccountingYear { get; } = accountingYear;

    public List<Stamp> Stamps { get; } = [];
    
    protected bool Equals(StampCard other)
    {
        return IssuedTo.Equals(other.IssuedTo) && AccountingYear == other.AccountingYear;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StampCard)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IssuedTo, AccountingYear);
    }
}