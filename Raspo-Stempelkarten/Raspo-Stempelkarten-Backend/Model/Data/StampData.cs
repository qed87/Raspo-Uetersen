namespace Raspo_Stempelkarten_Backend.Model.Data;

public class StampData
{
    public Guid Id { get; set; }
    
    public Guid StampCardId { get; set; }

    public string IssuedBy { get; set; } = null!;

    public string? Reason { get; set; }
}