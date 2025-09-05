namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardCreated
{
    public required Guid Id { get; set; }
    
    public string Recipient { get; set; } = null!;
    
    public string IssuedBy { get; set; } = null!;
    
    public int MinStamps { get; set; }
    
    public int MaxStamps { get; set; }
}