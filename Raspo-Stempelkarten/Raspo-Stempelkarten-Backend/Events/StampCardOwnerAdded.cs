namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardOwnerAdded : UserEvent
{
    public string Name { get; set; } = null!;
    public Guid StampCardId { get; set; }
}