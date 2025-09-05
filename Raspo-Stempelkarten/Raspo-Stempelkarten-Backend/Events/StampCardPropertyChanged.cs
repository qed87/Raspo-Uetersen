namespace Raspo_Stempelkarten_Backend.Events;

public class StampCardPropertyChanged
{
    public required Guid StampCardId { get; set; }
    
    public required string Name { get; set; }  = null!;
    
    public required object? Value { get; set; }
    
}