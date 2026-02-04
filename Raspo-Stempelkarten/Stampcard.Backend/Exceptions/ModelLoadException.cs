namespace StampCard.Backend.Exceptions;

/// <inheritdoc />
public class ModelLoadException : ModelException
{
    public ModelLoadException() : base()
    {
    }
    
    public ModelLoadException(string message) : base(message)
    {
    }
}