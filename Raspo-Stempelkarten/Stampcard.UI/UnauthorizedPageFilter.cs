using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Stampcard.UI;

/// <summary>
/// The backend answers 401 once the access token is no longer accepted. The user logs in again
/// and returns to the page, a redirect to another page would only run into the same 401 again.
/// </summary>
public class UnauthorizedPageFilter : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception is not UnauthorizedAccessException) return;

        var request = context.HttpContext.Request;
        // a posted form cannot be repeated after the login, so the user returns to the page itself
        var returnUrl = HttpMethods.IsGet(request.Method) ? $"{request.PathBase}{request.Path}{request.QueryString}" : $"{request.PathBase}{request.Path}";
        executed.Result = new ChallengeResult(
            OpenIdConnectDefaults.AuthenticationScheme,
            new AuthenticationProperties { RedirectUri = returnUrl });
        executed.ExceptionHandled = true;
    }
}
