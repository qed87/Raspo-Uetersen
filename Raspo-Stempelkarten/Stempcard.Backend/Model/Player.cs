namespace Raspo_Stempelkarten_Backend.Model;

public class Player(Guid id, string firstName, string lastName, DateOnly birthdate, string birthplace, bool deleted = false)
{
    public Guid Id { get; set; } = id;
    public string FirstName { get; set; } = firstName;
    public string LastName { get; set; } = lastName;
    public DateOnly Birthdate { get; set; } = birthdate;
    
    public string Birthplace { get; set; } = birthplace;
    public bool Deleted { get; set; } = deleted;
    protected bool Equals(Player other)
    {
        return FirstName == other.FirstName && LastName == other.LastName && Birthdate.Equals(other.Birthdate);
    }
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Player)obj);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName, Birthdate);
    }
}