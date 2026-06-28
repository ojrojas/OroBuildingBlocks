// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

public static class DataProtectionExtensions
{
    private const string DefaultKeyFolder = "data-protection-keys";

    /// <summary>
    /// Adds and configures data protection with file system key persistence.
    /// For Redis or Azure Blob storage, reference the respective packages directly:
    /// <c>Microsoft.AspNetCore.DataProtection.StackExchangeRedis</c> or
    /// <c>Azure.Extensions.AspNetCore.DataProtection.Blobs</c> and call their
    /// extension methods on the builder returned by <c>AddDataProtection()</c>.
    /// </summary>
    public static IServiceCollection AddConfiguredDataProtection(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment, ILoggerFactory? loggerFactory = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var applicationName = configuration["DataProtection:ApplicationName"] ?? "OroIdentityShared";

        IDataProtectionBuilder dpBuilder = services.AddDataProtection().SetApplicationName(applicationName);
        ILogger? logger = loggerFactory?.CreateLogger("DataProtectionExtensions");
        logger?.LogInformation("Configuring Data Protection with application name: {AppName}", applicationName);

        PersistToFile(dpBuilder, environment, logger);

        return services;
    }

    private static void PersistToFile(IDataProtectionBuilder builder, IHostEnvironment env, ILogger? logger, string pathToFiles = DefaultKeyFolder)
    {
        var keysFolder = Path.Combine(env.ContentRootPath, pathToFiles);
        Directory.CreateDirectory(keysFolder);
        var fullPath = Path.GetFullPath(keysFolder);
        builder.PersistKeysToFileSystem(new DirectoryInfo(fullPath));
        logger?.LogInformation("DataProtection: persisted keys to file system at {KeysFolder}", fullPath);
    }
}
