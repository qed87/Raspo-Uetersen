namespace Raspo_Stempelkarten_Backend.Dtos;

public class StempelkartenUpdateDto
{
    public required Guid Id { get; set; }

    public required string ConcurrencyToken { get; set; } = null!;
    
    public required string Recipient { get; set; } = null!;

    public required string Team { get; set; } = null!;

    public required string Season { get; set; } = null!;

}