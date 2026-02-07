namespace Stampcard.Contracts.Dtos;

/// <summary>
/// Generic wrapper for message responses.
/// </summary>
public sealed class ResponseWrapperDto
{
    private ResponseWrapperDto()
    {
        Message = "Ok";
    }
    
    private ResponseWrapperDto(object data) : this()
    {
        Data = data;
    }
    
    private ResponseWrapperDto(bool hasError, string message)
    {
        HasError = hasError;
        Message = message;
    }
    
    /// <summary>
    /// The response data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Indicates whether the response has an error.
    /// </summary>
    public bool HasError { get; set; } = false;
    
    /// <summary>
    /// The error message.
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// Creates a response object. 
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns></returns>
    public static ResponseWrapperDto Ok(object data)
    {
        return new ResponseWrapperDto(data);
    }
    
    /// <summary>
    /// Creates a empty response object. 
    /// </summary>
    /// <returns></returns>
    public static ResponseWrapperDto Ok()
    {
        return new ResponseWrapperDto();
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>The response dto.</returns>
    public static ResponseWrapperDto Fail(string message)
    {
        return new ResponseWrapperDto(hasError: true, message);
    }
}