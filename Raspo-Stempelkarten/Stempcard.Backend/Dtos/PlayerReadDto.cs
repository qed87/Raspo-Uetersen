namespace Raspo_Stempelkarten_Backend.Dtos;

public class PlayerReadDto
{
    public Guid Id { get; set; }
    
    public string FirstName { get; set; }
    
    public string Surname { get; set; }
    
    public DateOnly BirthDate { get; set; }
}