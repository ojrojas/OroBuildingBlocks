// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

/// <summary>
/// Identity endpoint options for configuring authentication routes.
/// </summary>
public class IdentityEndpointOptions
{
    /// <summary>
    /// Path for the login endpoint.
    /// </summary>
    public string LoginPath { get; set; } = "/account/login";
    /// <summary>
    /// Path for the logout endpoint.
    /// </summary>
    public string LogoutPath { get; set; } = "/account/logout";
    /// <summary>
    /// Path for the callback endpoint.
    /// </summary>
    public string CallbackPath { get; set; } = "/signin-oidc";
    /// <summary>
    /// Path for the signout callback endpoint.
    /// </summary>
    public string SignoutCallbackPath { get; set; } = "/signout-callback-oidc";
    /// <summary>
    /// Default redirect URI after authentication.
    /// </summary>
    public string DefaultRedirectUri { get; set; } = "/";
}

/// <summary>
/// Extension methods for mapping identity-related endpoints.
/// </summary>
public static class IdentityRouteExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        /// <summary>
        /// Maps identity endpoints for login, logout, and callback.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public IEndpointRouteBuilder MapIdentityEndpoints(Action<IdentityEndpointOptions>? configureOptions = null)
        {
            var options = new IdentityEndpointOptions();
            configureOptions?.Invoke(options);

            // Endpoint de Login (Challenge)
            app.MapGet(options.LoginPath, (string? returnUrl) =>
            {
                var properties = new AuthenticationProperties
                {
                    RedirectUri = returnUrl ?? options.DefaultRedirectUri
                };
                return Results.Challenge(properties, [OpenIddictClientAspNetCoreDefaults.AuthenticationScheme]);
            });

            // Endpoint de Logout (SignOut)
            // support receiving returnUrl to redirect after logout
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
            });

            // Endpoint Callback (signin-oidc)
            app.MapMethods(options.CallbackPath, [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
            {
                var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
                if (!result.Succeeded || result.Principal == null)
                {
                    return Results.Problem("External authentication failed.");
                }

                var identity = new ClaimsIdentity(result.Principal.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, result.Properties ?? new AuthenticationProperties());

                var redirectUri = result.Properties?.RedirectUri;
                if (string.IsNullOrEmpty(redirectUri) || !Uri.IsWellFormedUriString(redirectUri, UriKind.RelativeOrAbsolute))
                {
                    redirectUri = options.DefaultRedirectUri;
                }

                return Results.Redirect(redirectUri);
            }).DisableAntiforgery();

            return app;
        }
    }

    /// <summary>
    /// Get authentication properties with proper return URL handling.
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        // TODO: Use HttpContext.Request.PathBase instead.
        const string pathBase = "/";

        // Prevent open redirects.
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}


