using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Stampcard.UI.Pages;

public class LogoutModel : PageModel
{
    public IActionResult OnGetAsync(string returnUrl)
    {
        return SignOut(new AuthenticationProperties
            {
                RedirectUri = returnUrl
            },
            // Clear auth cookie
            CookieAuthenticationDefaults.AuthenticationScheme,
            // Redirect to OIDC provider signout endpoint
            OpenIdConnectDefaults.AuthenticationScheme);
    }
}