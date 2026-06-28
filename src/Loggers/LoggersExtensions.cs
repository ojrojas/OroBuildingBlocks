// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.Loggers;

/// <summary>
/// Extension methods for configuring Serilog with Seq integration.
/// </summary>
public static class LoggersExtensions
{
    /// <summary>
    /// Configures logging with Serilog, Seq, and debug output (in development).
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <param name="configuration">The application configuration, used to read Seq settings.</param>
    public static void AddServicesWritersLogger(
        this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        IConfigurationSection seqConfig = configuration.GetSection("Seq");

        if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddDebug();
        }

        if (seqConfig.Exists())
        {
            builder.AddSeqEndpoint("seq");
        }

        builder.Services.AddSerilog();
        builder.Services.AddLogging(options => options.AddSeq());
    }
}
