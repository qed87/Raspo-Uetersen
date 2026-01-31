namespace Raspo_Stempelkarten_Backend.Dtos;

public class TeamReadDto
{
    public string Id { get; set; }
    
    public string Club { get; set; }
    
    public string Name { get; set; }
    
    public string IssuedBy { get; set; }
    
    public string IssuedDate { get; set; }
}