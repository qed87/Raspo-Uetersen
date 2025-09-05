namespace Raspo_Stempelkarten_Backend.Model.Data;

public class StampCardData
{
    public string Team { get; set; }
    
    
    public string Season { get; set; }
    
    public Guid Id { get; set; }
    
    public string Recipient { get; set; }  = null!;

    public int MaxStamps { get; set; }

    public int MinStamps { get; set; }
    
    public string IssuedBy { get; set; }  = null!;
    
    public List<string> Owners { get; set; }
}