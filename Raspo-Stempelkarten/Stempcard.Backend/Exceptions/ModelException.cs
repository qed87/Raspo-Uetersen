namespace Raspo_Stempelkarten_Backend.Exceptions;

/// <summary>
/// Base exception for model errors.
/// </summary>
public abstract class ModelException : Exception
{
    public ModelException()
    {
    }
    
    public ModelException(string message) 
        : base(message)
    {
    }
}