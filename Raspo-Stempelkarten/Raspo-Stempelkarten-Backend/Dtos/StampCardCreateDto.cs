namespace Raspo_Stempelkarten_Backend.Dtos;

public class StampCardCreateDto
{
    public short AccountingYear { get; set; }
    
    public Guid IssuedTo { get; set; }
    
}