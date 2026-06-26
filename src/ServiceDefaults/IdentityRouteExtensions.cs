// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

public class IdentityEndpointOptions
{
    public string LoginPath { get; set; } = "/account/login";
    public string LogoutPath { get; set; } = "/account/logout";
    public string CallbackPath { get; set; } = "/signin-oidc";
    public string SignoutCallbackPath { get; set; } = "/signout-callback-oidc";
    public string DefaultRedirectUri { get; set; } = "/";
    public bool SignInLocal { get; set; } = true;
}

public static class IdentityRouteExtensions
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app, Action<IdentityEndpointOptions>? configureOptions = null)
    {
        var options = new IdentityEndpointOptions();
        configureOptions?.Invoke(options);

        app.MapGet(options.LoginPath, (string? returnUrl, bool localSignIn = false) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? options.DefaultRedirectUri
            };
            if (localSignIn)
            {
                properties.Parameters["local_signin"] = "true";
            }
            return Results.Challenge(properties, new[] { OpenIddictClientAspNetCoreDefaults.AuthenticationScheme });
        });

        app.MapPost(options.LogoutPath, (string? returnUrl, HttpContext context) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = options.SignoutCallbackPath
            };

            if (!string.IsNullOrEmpty(returnUrl))
            {
                properties.Items["returnUrl"] = returnUrl;
            }

            return Results.SignOut(properties, [
                OpenIddictClientAspNetCoreDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            ]);
        }).DisableAntiforgery();

        app.MapMethods(options.CallbackPath, new[] { HttpMethods.Get, HttpMethods.Post }, async (HttpContext context) =>
        {
            var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded || result.Principal == null)
                return Results.Problem("External authentication failed.");

            var redirectUri = result.Properties?.RedirectUri;
            if (string.IsNullOrEmpty(redirectUri) || !Uri.IsWellFormedUriString(redirectUri, UriKind.RelativeOrAbsolute))
                redirectUri = options.DefaultRedirectUri;

            var doLocalSignIn = false;
            if (Uri.TryCreate(redirectUri, UriKind.RelativeOrAbsolute, out var r))
            {
                doLocalSignIn = !r.IsAbsoluteUri
                    || (string.Equals(r.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase)
                        && r.Port == (context.Request.Host.Port ?? (context.Request.IsHttps ? 443 : 80)));
            }

            if (options.SignInLocal && doLocalSignIn)
            {
                var shouldLocalSignIn = false;
                if (result.Properties != null && result.Properties.Parameters.TryGetValue("local_signin", out var obj) && obj is string s && bool.TryParse(s, out var b))
                {
                    shouldLocalSignIn = b;
                }

                var principal = result.Principal;
                if (principal is null)
                    return Results.Redirect(redirectUri);

                var currentSub = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var incomingSub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (shouldLocalSignIn && currentSub != incomingSub)
                {
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, result.Properties ?? new AuthenticationProperties());
                }
            }

            return Results.Redirect(redirectUri);

        }).DisableAntiforgery();

        return app;
    }
}
