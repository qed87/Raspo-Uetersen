namespace Raspo_Stempelkarten_Backend.Dtos;

public record StampCardReadDetailsDto
{
    public StampCardReadDetailsDto(Guid id, Guid memberId, short accountingYear, string issuer, DateTimeOffset issuedOn)
    {
        Id = id;
        MemberId = memberId;
        AccountingYear = accountingYear;
        Issuer = issuer;
        IssuedOn = issuedOn;
    }
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public short AccountingYear { get; set; }
    public string Issuer { get; set; }
    public DateTimeOffset IssuedOn { get; set; }
    public List<StampReadDto> Stamps { get; set; } = [];
}