// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.Loggers;

public static class LoggerPrinter
{
    public static Serilog.ILogger CreateSerilogLogger(string key, string value, IConfiguration configuration)
    {
        var seqEndpoint = configuration["ConnectionStrings:Seq"];
        var loggerBuilding = new LoggerConfiguration()
              .MinimumLevel.Verbose()
              .Enrich.WithProperty(key, value)
              .Enrich.FromLogContext()
              .WriteTo.Console();

        if(!string.IsNullOrWhiteSpace(seqEndpoint))
        {
            loggerBuilding.WriteTo.Seq(seqEndpoint);
        }

        var logger = loggerBuilding.CreateLogger();

        Serilog.Debugging.SelfLog.Enable(Console.Error);

        return logger;
    }
}
