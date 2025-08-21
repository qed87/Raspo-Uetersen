namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardUpdated : UserEvent
{
    public required string Team { get; set; } = null!;
    
    public required string Season { get; set; } = null!;
    
    public required string Recipient { get; set; } = null!;
    
    public int MinStamps { get; set; }
    
    public int MaxStamps { get; set; }
    
    
    public string ConcurrencyToken { get; set; } = null!;
    
    public string[] AdditionalOwners { get; set; } = null!;
}