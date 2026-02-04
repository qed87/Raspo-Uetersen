namespace StampCard.Backend.Model;

/// <summary>
/// A coach of the team.
/// </summary>
public class Coach
{
    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the issuer.
    /// </summary>
    public string Issuer { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the date the coach is issued on.
    /// </summary>
    public DateTimeOffset IssuedOn { get; set; }

    /// <summary>
    /// Compares to coaches.
    /// </summary>
    protected bool Equals(Coach other)
    {
        return Email == other.Email;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Coach)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Email.GetHashCode();
    }
}