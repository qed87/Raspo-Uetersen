namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardDeleted : UserEvent
{
    public string ConcurrencyToken { get; set; } = null!;
}