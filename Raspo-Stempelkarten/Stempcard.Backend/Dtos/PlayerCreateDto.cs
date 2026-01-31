namespace Raspo_Stempelkarten_Backend.Dtos;

public class PlayerCreateDto
{
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public DateOnly Birthdate { get; set; }
    
    public string Birthplace { get; set; }
}