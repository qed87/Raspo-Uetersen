namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardDeleted
{
    public required Guid Id { get; set; }
    
    public string IssuedBy { get; set; }
}