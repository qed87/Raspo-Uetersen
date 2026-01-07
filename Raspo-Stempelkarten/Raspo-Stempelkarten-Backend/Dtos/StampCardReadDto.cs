namespace Raspo_Stempelkarten_Backend.Dtos;

public record StampCardReadDto
{
    public Guid Id { get; set; }
    
    public Guid IssuedTo { get; set; }

    public DateTimeOffset IssuedAt { get; set; }

    public short AccountingYear { get; set; }
}