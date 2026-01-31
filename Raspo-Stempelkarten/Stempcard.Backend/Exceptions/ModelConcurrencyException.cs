namespace Raspo_Stempelkarten_Backend.Exceptions;

/// <summary>
/// This exception is thrown if the model has been updated in the meantime.
/// </summary>
public class ModelConcurrencyException : ModelException
{
    public ModelConcurrencyException() 
        : base("Versionskonflikt: Es liegt eine neuere Version des Modells vor.")
    {
    }
}