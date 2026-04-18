// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServiceDefaults;

public static class DataProtectionExtensions
{
    /// <summary>
    /// Adds and configures data protection services based on configuration settings, with support for file system, Redis, or Azure Blob storage providers. Falls back to file system if provider configuration is missing or if required extensions are not available at runtime. Logs configuration decisions and errors to the console for visibility.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="environment"></param>
    /// <returns></returns>
    public static IServiceCollection AddConfiguredDataProtection(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var applicationName = configuration["DataProtection:ApplicationName"] ?? "OroIdentityShared";
        var provider = configuration["DataProtection:Provider"] ?? "File";

        var dpBuilder = services.AddDataProtection().SetApplicationName(applicationName);
        var logger = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("DataProtectionExtensions");
        logger.LogInformation("Configuring Data Protection with provider: {Provider} and application name: {AppName}", provider, applicationName);

        static void PersistToFile(IDataProtectionBuilder builder, IHostEnvironment env, string pathToFiles = "data-protection-keys", ILogger? logger = null)
        {
            var keysFolder = Path.Combine(env.ContentRootPath, pathToFiles);
            Directory.CreateDirectory(keysFolder);
            builder.PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysFolder)));
            logger?.LogInformation("DataProtection: persisted keys to file system at {KeysFolder}", Path.GetFullPath(keysFolder));
        }

        if (string.Equals(provider, "Redis", StringComparison.OrdinalIgnoreCase))
        {
            var redisConfig = configuration["DataProtection:Redis:Configuration"];
            if (string.IsNullOrWhiteSpace(redisConfig))
            {
                logger.LogWarning("DataProtection provider 'Redis' selected but DataProtection:Redis:Configuration is empty. Falling back to file system.");
                PersistToFile(dpBuilder, environment, logger: logger);
                return services;
            }

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                var extType = assemblies
                    .Select(a => a.GetType("Microsoft.AspNetCore.DataProtection.StackExchangeRedis.DataProtectionBuilderExtensions"))
                    .FirstOrDefault(t => t != null);

                var multiplexerType = assemblies
                    .Select(a => a.GetType("StackExchange.Redis.ConnectionMultiplexer"))
                    .FirstOrDefault(t => t != null);

                if (extType != null && multiplexerType != null)
                {
                    var connectMethod = multiplexerType.GetMethod("Connect", new[] { typeof(string) });
                    var multiplexer = connectMethod.Invoke(null, new object[] { redisConfig });
                    var getDbMethod = multiplexerType.GetMethod("GetDatabase", Type.EmptyTypes);
                    var database = getDbMethod.Invoke(multiplexer, null);

                    var persistMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(m => m.Name == "PersistKeysToStackExchangeRedis");

                    if (persistMethod != null)
                    {
                        // Expected signature: (IDataProtectionBuilder builder, object database, string key)
                        persistMethod.Invoke(null, new object[] { dpBuilder, database, "DataProtection-Keys" });
                        logger.LogInformation("DataProtection: configured Redis key persistence via runtime extension.");
                        return services;
                    }
                }

                logger.LogWarning("DataProtection: Redis extension or StackExchange.Redis not available at runtime. Falling back to file system.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataProtection: error configuring Redis provider. Falling back to file system.");
            }

            PersistToFile(dpBuilder, environment, logger: logger);
            return services;
        }

        if (string.Equals(provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
        {
            var conn = configuration["DataProtection:AzureBlob:ConnectionString"];
            var containerName = configuration["DataProtection:AzureBlob:ContainerName"] ?? "dataprotection";

            if (string.IsNullOrWhiteSpace(conn))
            {
                logger.LogWarning("DataProtection provider 'AzureBlob' selected but connection string is empty. Falling back to file system.");
                PersistToFile(dpBuilder, environment, logger: logger);
                return services;
            }

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                var extType = assemblies
                    .Select(a => a.GetType("Azure.Extensions.AspNetCore.DataProtection.Blobs.DataProtectionBuilderExtensions"))
                    .FirstOrDefault(t => t != null);

                var blobServiceType = assemblies
                    .Select(a => a.GetType("Azure.Storage.Blobs.BlobServiceClient"))
                    .FirstOrDefault(t => t != null);

                if (extType != null && blobServiceType != null)
                {
                    var ctor = blobServiceType.GetConstructor([typeof(string)]);
                    var blobServiceClient = ctor.Invoke([conn]);
                    var getContainerMethod = blobServiceType.GetMethod("GetBlobContainerClient", [typeof(string)]);
                    var containerClient = getContainerMethod.Invoke(blobServiceClient, [containerName]);

                    var persistMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(m => m.Name == "PersistKeysToAzureBlobStorage");

                    if (persistMethod != null)
                    {
                        // Expected signature: (IDataProtectionBuilder builder, BlobContainerClient containerClient, string blobName)
                        persistMethod.Invoke(null, [dpBuilder, containerClient, "keys.xml"]);
                        logger.LogInformation("DataProtection: configured Azure Blob key persistence via runtime extension.");
                        return services;
                    }
                }

                logger.LogWarning("DataProtection: Azure Blob extension not available at runtime. Falling back to file system.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataProtection: error configuring AzureBlob provider. Falling back to file system.");
            }

            PersistToFile(dpBuilder, environment, logger: logger);
            return services;
        }

        // Default: file system
        PersistToFile(dpBuilder, environment, logger: logger);
        return services;
    }
}
