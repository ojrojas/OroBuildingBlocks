// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

/// <summary>
/// Extension methods for <see cref="ClaimsPrincipal"/> to retrieve common user information.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID from the <c>sub</c> claim.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user ID, or <c>null</c> if the claim is not present.</returns>
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst("sub")?.Value;

    /// <summary>
    /// Gets the user name from the <see cref="ClaimTypes.Name"/> claim.
    /// </summary>
    /// <param name="principal">The claims principal.</param>
    /// <returns>The user name, or <c>null</c> if the claim is not present.</returns>
    public static string? GetUserName(this ClaimsPrincipal principal) =>
        principal.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
}
