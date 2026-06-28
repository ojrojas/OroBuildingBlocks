// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
using Serilog.Core;

namespace OroBuildingBlocks.Loggers;

/// <summary>
/// Helper methods for creating pre-configured Serilog loggers.
/// </summary>
public static class LoggerPrinter
{
    /// <summary>
    /// Creates a Serilog logger configured with console and optional Seq sinks.
    /// </summary>
    /// <param name="key">The property key to enrich log events with (e.g. "Application").</param>
    /// <param name="value">The property value to enrich log events with (e.g. "MyService").</param>
    /// <param name="configuration">The application configuration, used to read <c>ConnectionStrings:Seq</c>.</param>
    /// <returns>A configured Serilog <see cref="Serilog.ILogger"/> instance.</returns>
    public static Serilog.ILogger CreateSerilogLogger(string key, string value, IConfiguration configuration)
    {
        var seqEndpoint = configuration["ConnectionStrings:Seq"];
        LoggerConfiguration loggerBuilding = new LoggerConfiguration()
              .MinimumLevel.Verbose()
              .Enrich.WithProperty(key, value)
              .Enrich.FromLogContext()
              .WriteTo.Console();

        if(!string.IsNullOrWhiteSpace(seqEndpoint))
        {
            loggerBuilding.WriteTo.Seq(seqEndpoint);
        }

        Logger logger = loggerBuilding.CreateLogger();

        Serilog.Debugging.SelfLog.Enable(Console.Error);

        return logger;
    }
}
