namespace Stampcard.UI.Clients;

public class ResponseWrapperDto<T> : ResponseWrapperDto
{
    /// <summary>
    /// The response data.
    /// </summary>
    public T Data { get; set; }

    public new static ResponseWrapperDto<T> Fail(string error)
    {
        return new ResponseWrapperDto<T>()
        {
            HasError = true,
            Message = error
        };
    }
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

    /// <summary>
    /// The revision of the team stream, only set for responses that refer to exactly one team.
    /// </summary>
    public long? ConcurrencyToken { get; set; }

    public static ResponseWrapperDto Fail(string error)
    {
        return new ResponseWrapperDto()
        {
            HasError = true,
            Message = error
        };
    }
}
