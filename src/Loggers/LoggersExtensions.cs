// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.Loggers;

public static class LoggersExtensions
{
    public static void AddServicesWritersLogger(
        this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        var seqConfig = configuration.GetSection("Seq");

        builder.Logging.ClearProviders();

        if(builder.Environment.IsDevelopment())
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