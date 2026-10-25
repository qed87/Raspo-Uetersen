using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Stampcard.UI;

/// <summary>
/// Users of the accounting only see the accounting page. Every other page, the team overview included,
/// redirects them to it. Pages without team data stay available to them.
/// </summary>
public class AccountingPageFilter : IAsyncPageFilter
{
    private static readonly HashSet<string> PagesForAccounting = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Accounting", "/Help", "/Logout", "/Error", "/SharedStampCard"
    };

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (context.HttpContext.User.IsRestrictedToAccounting()
            && !PagesForAccounting.Contains(context.ActionDescriptor.ViewEnginePath))
        {
            context.Result = new RedirectToPageResult("/Accounting");
            return;
        }

        await next();
    }
}
