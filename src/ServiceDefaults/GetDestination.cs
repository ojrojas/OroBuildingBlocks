// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

/// <summary>
/// Provides claim destination mapping for OpenIddict token issuance.
/// Determines whether a claim should be included in the access token, identity token, or both.
/// </summary>
public static class GetDestination
{
    /// <summary>
    /// Returns the token destinations for a given claim.
    /// Name, subject, email, and role claims are included in both access and identity tokens.
    /// All other claims are included only in the access token.
    /// </summary>
    /// <param name="claim">The claim to evaluate.</param>
    /// <returns>A collection of destination strings (e.g. "access_token", "id_token").</returns>
    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        return claim.Type switch
        {
            Claims.Name or
            Claims.Subject or
            Claims.Email or
            Claims.Role
                => new[] { Destinations.AccessToken, Destinations.IdentityToken },

            _ => [Destinations.AccessToken],
        };
    }
}
