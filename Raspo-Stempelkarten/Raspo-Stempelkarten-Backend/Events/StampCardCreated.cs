namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardCreated : UserEvent
{
    public string Recipient { get; set; } = null!;
    
    public string Owner { get; set; } = null!;

    public string Team { get; set; } = null!;

    public string Season { get; set; } = null!;
    
    public int MinStamps { get; set; }
    
    public int MaxStamps { get; set; }
}