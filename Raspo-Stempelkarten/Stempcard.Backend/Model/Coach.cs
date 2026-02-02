namespace Raspo_Stempelkarten_Backend.Model;

/// <summary>
/// A coach of the team.
/// </summary>
public class Coach
{
    public string Email { get; set; }
    
    public string Issuer { get; set; }
    
    public DateTimeOffset IssuedOn { get; set; }

    protected bool Equals(Coach other)
    {
        return Email == other.Email;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Coach)obj);
    }

    public override int GetHashCode()
    {
        return Email.GetHashCode();
    }
}