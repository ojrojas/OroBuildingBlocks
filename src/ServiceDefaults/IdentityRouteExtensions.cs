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

    /// <summary>
    /// When true, the callback endpoint will perform a local signin if the redirect URI is local.
    /// When false, the callback only redirects and the client is responsible for signing in.
    /// </summary>
    public bool SignInLocal { get; set; } = true;
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
            app.MapMethods(options.CallbackPath, new[] { HttpMethods.Get, HttpMethods.Post }, async (HttpContext context) =>
            {
                var result = await context.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme); 
                if (!result.Succeeded || result.Principal == null) 
                    return Results.Problem("External authentication failed.");

                var redirectUri = result.Properties?.RedirectUri;
                if (string.IsNullOrEmpty(redirectUri) || !Uri.IsWellFormedUriString(redirectUri, UriKind.RelativeOrAbsolute))
                    redirectUri = options.DefaultRedirectUri;

                // Decide si el signin debe hacerse localmente o en la app destino
                var doLocalSignIn = false;
                if (Uri.TryCreate(redirectUri, UriKind.RelativeOrAbsolute, out var r))
                {
                    doLocalSignIn = !r.IsAbsoluteUri
                        || (string.Equals(r.Host, context.Request.Host.Host, StringComparison.OrdinalIgnoreCase)
                            && r.Port == (context.Request.Host.Port ?? (context.Request.IsHttps ? 443 : 80)));
                }

                if (options.SignInLocal && doLocalSignIn)
                {
                    // Only perform local sign-in when explicitly requested by the initiating challenge.
                    // This avoids creating unintended authentication cookies in client apps.
                    var shouldLocalSignIn = false;
                    if (result.Properties != null && result.Properties.Parameters.TryGetValue("local_signin", out var obj) && obj is string s && bool.TryParse(s, out var b))
                    {
                        shouldLocalSignIn = b;
                    }

                    var currentSub = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var incomingSub = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (shouldLocalSignIn && currentSub != incomingSub)
                    {
                        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal, result.Properties ?? new AuthenticationProperties());
                    }
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


