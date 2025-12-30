namespace Raspo_Stempelkarten_Backend.Dtos;

public class StampCardReadDto
{
    public Guid Id { get; set; }

    public string Season { get; set; } = null!;

    public string Team { get; set; } = null!;
    
    public required string Recipient { get; set; } = null!;
    
    public string[] Owners { get; set; } = [];

    public string IssuedBy { get; set; } = null!;

    public int MinStamps { get; set; } = 12;

    public int MaxStamps { get; set; } = 12;
}