namespace Raspo_Stempelkarten_Backend.Dtos;

public class StampCardCreateDto
{
    public required string Recipient { get; set; } = null!;

    public required string Team { get; set; } = null!;

    public required string Season { get; set; } = null!;
    
    public string[] AdditionalOwner { get; set; } = [];

    public int MinStamps { get; set; } = 12;

    public int MaxStamps { get; set; } = 12;
}