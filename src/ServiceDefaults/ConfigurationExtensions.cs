// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServiceDefaults;

/// <summary>
/// Extension methods for IConfiguration to retrieve required configuration values.
/// </summary>
public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        /// <summary>
        /// Gets a required configuration value by name, throwing an exception if not found.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string GetRequiredValue(string name) =>
            configuration[name] ?? throw new InvalidOperationException($"Configuration missing value for: {(configuration is IConfigurationSection s ? s.Path + ":" + name : name)}");
    }
}