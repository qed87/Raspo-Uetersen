namespace StampCard.Backend.Model;

/// <summary>
/// A member stamp card for a given accounting year.
/// </summary>
public class StampCard(Guid id, Guid playerId, string issuer, DateTimeOffset issuedOn, short accountingYear)
{
    /// <inheritdoc />
    public StampCard(Guid playerId, short accountingYear, string issuer, DateTimeOffset issuedOn)
        : this(Guid.NewGuid(), playerId, issuer, issuedOn, accountingYear)
    {
    }

    /// <summary>
    /// Gets or sets the Id.
    /// </summary>
    public Guid Id { get; set; } = id;
    
    /// <summary>
    /// Gets or sets the player id.
    /// </summary>
    public Guid PlayerId { get; } = playerId;
    
    /// <summary>
    /// Gets or sets the accounting year.
    /// </summary>
    public short AccountingYear { get; } = accountingYear;
    
    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    public string Issuer { get; } = issuer;
    
    /// <summary>
    /// Gets or sets the date this stamp card is issued on.
    /// </summary>
    public DateTimeOffset IssuedOn { get; } = issuedOn;
    
    /// <summary>
    /// Gets mor sets the stamps of this stamp card.
    /// </summary>
    public List<Stamp> Stamps { get; } = [];
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected bool Equals(StampCard other)
    {
        return PlayerId.Equals(other.PlayerId) && AccountingYear == other.AccountingYear;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StampCard)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(PlayerId, AccountingYear);
    }
}