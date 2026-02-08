namespace StampCard.Backend.Model;

/// <summary>
/// Add a player of the team.
/// </summary>
public class Player(Guid id, string firstName, string lastName, DateOnly birthdate, string birthplace, bool active = true)
{
    /// <summary>
    /// Gets or sets the player id.
    /// </summary>
    public Guid Id { get; set; } = id;
    
    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = firstName;
    
    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = lastName;
    
    /// <summary>
    /// Gets or sets the birthdate.
    /// </summary>
    public DateOnly Birthdate { get; set; } = birthdate;
    
    /// <summary>
    /// Gets or sets the birthdate.
    /// </summary>
    public string Birthplace { get; set; } = birthplace;
    
    /// <summary>
    /// Gets or sets a value indicating whether the player is active or inactive.
    /// </summary>
    public bool Active { get; set; } = active;
    
    /// <summary>
    /// Compares to another player. 
    /// </summary>
    /// <param name="other">another player</param>
    protected bool Equals(Player other)
    {
        return FirstName == other.FirstName && LastName == other.LastName && Birthdate.Equals(other.Birthdate) 
               && Birthplace.Equals(other.Birthplace);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Player)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName, Birthdate, Birthplace);
    }
}