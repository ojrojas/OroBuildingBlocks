using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OroBuildingBlocks.ServiceDefaults
{
    public static class DataProtectionExtensions
    {
        /// <summary>
        /// Configure IDataProtection persistence based on configuration.
        /// Supported providers: File (default), Redis, AzureBlob.
        /// If Redis/AzureBlob packages are not present at runtime the method falls back to filesystem and logs a message.
        /// </summary>
        public static IServiceCollection AddConfiguredDataProtection(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (environment == null) throw new ArgumentNullException(nameof(environment));

            var applicationName = configuration["DataProtection:ApplicationName"] ?? "OroIdentityShared";
            var provider = configuration["DataProtection:Provider"] ?? "File";

            var dpBuilder = services.AddDataProtection().SetApplicationName(applicationName);

            static void PersistToFile(IDataProtectionBuilder builder, IHostEnvironment env)
            {
                var keysFolder = Path.Combine(env.ContentRootPath, "..", "..", "..", "..", "data-protection-keys");
                Directory.CreateDirectory(keysFolder);
                builder.PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysFolder)));
                Console.WriteLine($"DataProtection: persisted keys to file system at {Path.GetFullPath(keysFolder)}");
            }

            if (string.Equals(provider, "Redis", StringComparison.OrdinalIgnoreCase))
            {
                var redisConfig = configuration["DataProtection:Redis:Configuration"];
                if (string.IsNullOrWhiteSpace(redisConfig))
                {
                    Console.WriteLine("DataProtection provider 'Redis' selected but DataProtection:Redis:Configuration is empty. Falling back to file system.");
                    PersistToFile(dpBuilder, environment);
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
                            Console.WriteLine("DataProtection: configured Redis key persistence via runtime extension.");
                            return services;
                        }
                    }

                    Console.WriteLine("DataProtection: Redis extension or StackExchange.Redis not available at runtime. Falling back to file system.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DataProtection: error configuring Redis provider: {ex.Message}. Falling back to file system.");
                }

                PersistToFile(dpBuilder, environment);
                return services;
            }

            if (string.Equals(provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            {
                var conn = configuration["DataProtection:AzureBlob:ConnectionString"];
                var containerName = configuration["DataProtection:AzureBlob:ContainerName"] ?? "dataprotection";

                if (string.IsNullOrWhiteSpace(conn))
                {
                    Console.WriteLine("DataProtection provider 'AzureBlob' selected but connection string is empty. Falling back to file system.");
                    PersistToFile(dpBuilder, environment);
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
                        var ctor = blobServiceType.GetConstructor(new[] { typeof(string) });
                        var blobServiceClient = ctor.Invoke(new object[] { conn });
                        var getContainerMethod = blobServiceType.GetMethod("GetBlobContainerClient", new[] { typeof(string) });
                        var containerClient = getContainerMethod.Invoke(blobServiceClient, new object[] { containerName });

                        var persistMethod = extType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(m => m.Name == "PersistKeysToAzureBlobStorage");

                        if (persistMethod != null)
                        {
                            // Expected signature: (IDataProtectionBuilder builder, BlobContainerClient containerClient, string blobName)
                            persistMethod.Invoke(null, new object[] { dpBuilder, containerClient, "keys.xml" });
                            Console.WriteLine("DataProtection: configured Azure Blob key persistence via runtime extension.");
                            return services;
                        }
                    }

                    Console.WriteLine("DataProtection: Azure Blob extension not available at runtime. Falling back to file system.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DataProtection: error configuring AzureBlob provider: {ex.Message}. Falling back to file system.");
                }

                PersistToFile(dpBuilder, environment);
                return services;
            }

            // Default: file system
            PersistToFile(dpBuilder, environment);
            return services;
        }
    }
}
