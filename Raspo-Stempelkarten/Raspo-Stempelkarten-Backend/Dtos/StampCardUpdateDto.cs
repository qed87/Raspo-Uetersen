namespace Raspo_Stempelkarten_Backend.Dtos;

public class StampCardUpdateDto
{
    public required Guid Id { get; set; }

    public required string ConcurrencyToken { get; set; } = null!;
    
    public required string Recipient { get; set; } = null!;

}