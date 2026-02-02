namespace Raspo_Stempelkarten_Backend.Model;

/// <summary>
/// A member stamp card for a given accounting year.
/// </summary>
public class StampCard(Guid id, Guid memberId, string issuer, DateTimeOffset issuedOn, short accountingYear)
{
    public StampCard(Guid memberId, short accountingYear, string issuer, DateTimeOffset issuedOn)
        : this(Guid.NewGuid(), memberId, issuer, issuedOn, accountingYear)
    {
    }

    public Guid Id { get; set; } = id;
    public Guid MemberId { get; } = memberId;
    public short AccountingYear { get; } = accountingYear;
    public string Issuer { get; } = issuer;
    public DateTimeOffset IssuedOn { get; } = issuedOn;
    public List<Stamp> Stamps { get; } = [];
    
    protected bool Equals(StampCard other)
    {
        return MemberId.Equals(other.MemberId) && AccountingYear == other.AccountingYear;
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
        return HashCode.Combine(MemberId, AccountingYear);
    }
}