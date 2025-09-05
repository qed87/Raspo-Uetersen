namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardOwnerRemoved
{
    public Guid StampCardId { get; set; }
    
    public string Name { get; set; } = null!;
}