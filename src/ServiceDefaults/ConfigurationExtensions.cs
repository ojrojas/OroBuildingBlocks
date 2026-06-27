// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

/// <summary>
/// Extension methods for <see cref="IConfiguration"/> to retrieve required values.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a configuration value by name, throwing <see cref="InvalidOperationException"/> if the value is not found.
    /// </summary>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="name">The configuration key.</param>
    /// <returns>The configuration value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the configuration key is missing.</exception>
    public static string GetRequiredValue(this IConfiguration configuration, string name) =>
        configuration[name] ?? throw new InvalidOperationException($"Configuration missing value for: {(configuration is IConfigurationSection s ? s.Path + ":" + name : name)}");
}
