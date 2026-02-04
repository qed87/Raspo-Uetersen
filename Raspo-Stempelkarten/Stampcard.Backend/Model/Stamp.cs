namespace StampCard.Backend.Model;

/// <summary>
/// A stamp on a stamp card.
/// </summary>
public class Stamp(Guid id, string reason, string issuer, DateTimeOffset issuedOn)
{
    /// <inheritdoc />
    public Stamp(string reason) 
        : this(Guid.NewGuid(), reason, "dbo", DateTimeOffset.UtcNow)
    {
    }

    /// <summary>
    /// Gets or sets id.
    /// </summary>
    public Guid Id { get; set; } = id;
    
    /// <summary>
    /// Gets or sets reason.
    /// </summary>
    public string Reason { get; set; } = reason;

    /// <summary>
    /// gets or sets the issuer.
    /// </summary>
    public string Issuer { get; set; } = issuer;

    /// <summary>
    /// Gets or sets date when the stamp was issued on.
    /// </summary>
    public DateTimeOffset IssuedOn { get; set; } = issuedOn;
    
}