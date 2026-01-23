namespace Raspo_Stempelkarten_Backend.Dtos;

public record StampCardReadDetailsDto
{
    public Guid Id { get; set; }
    
    public Guid IssuedTo { get; set; }

    public DateTimeOffset IssuedAt { get; set; }

    public short AccountingYear { get; set; }

    public List<StampReadDto> Stamps { get; set; } = [];
}