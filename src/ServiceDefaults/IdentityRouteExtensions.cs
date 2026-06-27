// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

/// <summary>
/// Configuration options for identity-related endpoint paths and behaviour.
/// </summary>
public class IdentityEndpointOptions
{
    /// <summary>
    /// Gets or sets the login endpoint path. Defaults to <c>/account/login</c>.
    /// </summary>
    public string LoginPath { get; set; } = "/account/login";

    /// <summary>
    /// Gets or sets the logout endpoint path. Defaults to <c>/account/logout</c>.
    /// </summary>
    public string LogoutPath { get; set; } = "/account/logout";

    /// <summary>
    /// Gets or sets the callback path used by the external OIDC provider. Defaults to <c>/signin-oidc</c>.
    /// </summary>
    public string CallbackPath { get; set; } = "/signin-oidc";

    /// <summary>
    /// Gets or sets the signout callback path. Defaults to <c>/signout-callback-oidc</c>.
    /// </summary>
    public string SignoutCallbackPath { get; set; } = "/signout-callback-oidc";

    /// <summary>
    /// Gets or sets the default redirect URI after authentication. Defaults to <c>/</c>.
    /// </summary>
    public string DefaultRedirectUri { get; set; } = "/";

    /// <summary>
    /// When <c>true</c>, the callback endpoint performs a local cookie sign-in
    /// when the redirect target is the same application. Defaults to <c>true</c>.
    /// </summary>
    public bool SignInLocal { get; set; } = true;
}

/// <summary>
/// Extension methods for mapping OpenIddict-based identity endpoints.
/// </summary>
public static class IdentityRouteExtensions
{
    /// <summary>
    /// Maps login, logout, and callback endpoints for OpenIddict authentication.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="configureOptions">An optional delegate to customise endpoint paths and behaviour.</param>
    /// <returns>The same route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app, Action<IdentityEndpointOptions>? configureOptions = null)
    {
        var options = new IdentityEndpointOptions();
        configureOptions?.Invoke(options);

        app.MapGet(options.LoginPath, (string? returnUrl, bool localSignIn = false) =>
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = ValidateLocalRedirect(returnUrl, options.DefaultRedirectUri)
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
            {
                return Results.Problem("External authentication failed.");
            }

            var redirectUri = ValidateLocalRedirect(result.Properties?.RedirectUri, options.DefaultRedirectUri);

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
                {
                    return Results.Redirect(redirectUri);
                }

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

    private static string ValidateLocalRedirect(string? returnUrl, string defaultRedirect)
    {
        if (string.IsNullOrEmpty(returnUrl))
        {
            return defaultRedirect;
        }

        // Only allow relative URIs to prevent open redirect attacks
        if (Uri.TryCreate(returnUrl, UriKind.Relative, out _))
        {
            return returnUrl;
        }

        // Allow absolute URIs that point to the same host
        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var absolute))
        {
            if (string.Equals(absolute.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                || absolute.IsLoopback)
            {
                return returnUrl;
            }
        }

        return defaultRedirect;
    }
}
