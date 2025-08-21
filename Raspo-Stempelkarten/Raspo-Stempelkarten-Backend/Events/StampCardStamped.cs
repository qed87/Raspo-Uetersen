namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardStamped : UserEvent
{
    public required string Reason { get; set; } = null!;
    
    public required string IssuedBy { get; set; } = null!;
}