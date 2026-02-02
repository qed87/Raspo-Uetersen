using System.Text;
using FluentResults;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens.Experimental;
using Raspo_Stempelkarten_Backend.Dtos;

namespace Raspo_Stempelkarten_Backend;

/// <summary>
/// Creates a http response from result objects.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts an authorization result to a fluent result error.
    /// </summary>
    /// <param name="authorizationResult">The authorization result.</param>
    public static Result ToResult(this AuthorizationResult authorizationResult)
    {
        if (authorizationResult.Succeeded) return Result.Ok();
        var sb = new StringBuilder();
        foreach (var failureReason in authorizationResult.Failure.FailureReasons)
            sb.AppendLine(failureReason.Message);
        return Result.Fail(sb.ToString());
    }
    
    /// <summary>
    /// Converts a valdiation result to a fluent result error.
    /// </summary>
    /// <param name="validationResult">The authorization result.</param>
    public static Result ToResult(this ValidationResult validationResult)
    {
        if (validationResult.IsValid) return Result.Ok();
        var sb = new StringBuilder();
        foreach (var reason in validationResult.Errors.Select(x => x.ErrorMessage))
            sb.AppendLine(reason);
        return Result.Fail(sb.ToString());
    }
    
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