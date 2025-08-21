namespace Raspo_Stempelkarten_Backend.Dtos;

public class StampCardDeleteDto
{
    public  Guid Id { get; set; }

    public required string ConcurrencyToken { get; set; } = null!;
}