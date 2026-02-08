namespace Stampcard.Contracts.Dtos;

/// <summary>
/// Stamp card read details.
/// </summary>
public record StampCardReadDetailsDto
{
    /// <summary>
    /// Create new stamp card read details.
    /// </summary>
    public StampCardReadDetailsDto(Guid id, Guid playerId, short accountingYear, string issuer, DateTimeOffset issuedOn)
    {
        Id = id;
        PlayerId = playerId;
        AccountingYear = accountingYear;
        Issuer = issuer;
        IssuedOn = issuedOn;
    }
    /// <summary>
    /// Gets or sets the stamp card id.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the player id.
    /// </summary>
    public Guid PlayerId { get; set; }
    /// <summary>
    /// Gets or sets the accounting year.
    /// </summary>
    public short AccountingYear { get; set; }
    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    public string Issuer { get; set; }
    /// <summary>
    /// Gets or sets the date when the issued stamp card was issued.
    /// </summary>
    public DateTimeOffset IssuedOn { get; set; }
    /// <summary>
    /// Gets or sets the stamps.
    /// </summary>
    public List<StampReadDto> Stamps { get; set; } = [];
}