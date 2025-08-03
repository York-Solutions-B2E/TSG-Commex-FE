using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Okta.AspNetCore;

namespace TSG_Commex_FE.Controllers;

[Route("[controller]/[action]")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthController : Controller
{
    public IActionResult SignIn([FromQuery] string returnUrl = "/")
    {
        if (User.Identity.IsAuthenticated)
        {
            return LocalRedirect(returnUrl);
        }

        return Challenge(OktaDefaults.MvcAuthenticationScheme);
    }

    public IActionResult SignOut([FromQuery] string returnUrl = "/")
    {
        if (!User.Identity.IsAuthenticated)
        {
            return LocalRedirect(returnUrl);
        }

        return SignOut(
            new AuthenticationProperties() { RedirectUri = "/" },
            new[]
            {
                OktaDefaults.MvcAuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme,
            });
    }
}