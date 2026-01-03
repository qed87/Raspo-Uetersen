namespace Raspo_Stempelkarten_Backend.Dtos;

public class PlayerCreateDto
{
    public string FirstName { get; set; }
    
    public string Surname { get; set; }
    
    public DateOnly Birthdate { get; set; }
}