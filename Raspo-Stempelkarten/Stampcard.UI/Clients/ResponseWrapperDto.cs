namespace Stampcard.UI.Clients;

public class ResponseWrapperDto<T>
{
    /// <summary>
    /// The response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Indicates whether the response has an error.
    /// </summary>
    public bool HasError { get; set; } = false;
    
    /// <summary>
    /// The error message.
    /// </summary>
    public string? Message { get; set; }
}

public class ResponseWrapperDto
{
    /// <summary>
    /// Indicates whether the response has an error.
    /// </summary>
    public bool HasError { get; set; } = false;
    
    /// <summary>
    /// The error message.
    /// </summary>
    public string? Message { get; set; }
}