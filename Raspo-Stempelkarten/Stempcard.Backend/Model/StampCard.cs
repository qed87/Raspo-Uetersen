namespace Raspo_Stempelkarten_Backend.Model;

public class StampCard(Guid id, Guid playerId, DateTimeOffset issuedDate, short accountingYear)
{
    public StampCard(Guid playerId, short accountingYear)
        : this(Guid.NewGuid(), playerId, DateTimeOffset.UtcNow, accountingYear)
    {
    }

    public Guid Id { get; set; } = id;
    
    public Guid PlayerId { get; } = playerId;
    
    public DateTimeOffset IssuedDate { get; } = issuedDate;

    public short AccountingYear { get; } = accountingYear;

    public List<Stamp> Stamps { get; } = [];
    
    protected bool Equals(StampCard other)
    {
        return PlayerId.Equals(other.PlayerId) && AccountingYear == other.AccountingYear;
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
        return HashCode.Combine(PlayerId, AccountingYear);
    }
}