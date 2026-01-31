using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend;

/// <summary>
/// Creates a http response from result objects.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Creates an http response from a result object.
    /// </summary>
    public static IActionResult ToHttpResponse(this Result result)
    {
        if(result.IsSuccess) return new OkObjectResult(ResponseWrapperDto.Ok());
        return new BadRequestObjectResult(ResponseWrapperDto.Fail(string.Join(Environment.NewLine, result.Errors.Select(error => error.Message))));
    }
    
    /// <summary>
    /// Creates an http response from a result object.
    /// </summary>
    public static IActionResult ToHttpResponse<T>(this Result<T> result)
    {
        if(result.IsSuccess) return new OkObjectResult(ResponseWrapperDto.Ok(result.ValueOrDefault!));
        return new BadRequestObjectResult(ResponseWrapperDto.Fail(string.Join(Environment.NewLine, result.Errors.Select(error => error.Message))));
    }
}