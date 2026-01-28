// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServiceDefaults;

/// <summary>
/// Extension methods for ClaimsPrincipal to retrieve user information.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal principal)
    {
        /// <summary>
        /// Gets the user ID from the claims principal.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        /// 
        public string? GetUserId()
            => principal.FindFirst("sub")?.Value;

        /// <summary>
        /// Gets the user name from the claims principal.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public string? GetUserName() =>
            principal.FindFirst(x => x.Type == ClaimTypes.Name)?.Value;
    }
}