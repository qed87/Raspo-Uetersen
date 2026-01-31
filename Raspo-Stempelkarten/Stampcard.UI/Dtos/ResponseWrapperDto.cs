namespace Stampcard.UI.Dtos;

public class ResponseWrapperDto : ResponseWrapperDto<object>
{
}

public class ResponseWrapperDto<T>
{
    public T Data { get; set; }
    
    public bool HasError { get; set; }
    
    public string Message { get; set; }
}