# OroBuildingBlocks

OroBuildingBlocks is a collection of reusable .NET components and infrastructure "building blocks" designed to accelerate the development of distributed systems and microservices. It centralizes common cross-cutting concerns, ensuring consistency and reducing boilerplate code across multiple services.

## Overview

This project provides a set of libraries that implement best practices for modern .NET applications, particularly those aligned with .NET Aspire and cloud-native architectures.

## Key Features

### 🚀 Service Defaults
The `ServiceDefaults` project simplifies the bootstrapping of essential service features:
- **Observability**: Pre-configured OpenTelemetry for Metrics, Tracing, and Logging.
- **Health Checks**: Standardized endpoints for liveness and readiness probes (`/alive` and `/health`).
- **Resilience**: Default HTTP client resilience handlers and service discovery integration.
- **Service Discovery**: Built-in support for .NET Service Discovery.

### 📝 Logging
The `Loggers` project provides utilities for structured logging:
- **Serilog Integration**: Easy setup of Serilog with Console and [Seq](https://datalust.co/seq) sinks.
- **Enrichment**: Automatic enrichment with application-specific properties and log context.

### 🔐 Identity & Security
Standardized ways to handle user identity:
- **Claims Extensions**: Helper methods to retrieve common information (like `UserId` and `UserName`) from `ClaimsPrincipal`.
- **Identity Routing**: Flexible mapping of OIDC-related endpoints (Login, Logout, Callbacks) to maintain consistent authentication flows.

### 🛠 Utility Extensions
Various helper methods to streamline development:
- **Configuration**: Extensions for `IConfiguration` to strictly enforce required configuration values.
- **Async Helpers**: Extensions for managing asynchronous task execution.

## Getting Started

To use these building blocks, reference the corresponding project in your service:

```bash
dotnet add reference src/ServiceDefaults/ServiceDefaults.csproj
dotnet add reference src/Loggers/Loggers.csproj
```

In your `Program.cs`, you can then use the extensions:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add standard service defaults
builder.AddServiceDefaults();

// Configure logging
var logger = LoggerPrinter.CreateSerilogLogger("AppName", "MyService", builder.Configuration);
builder.Host.UseSerilog(logger);

var app = builder.Build();

// Map standard endpoints
app.MapDefaultEndpoints();

app.Run();
```

## License

This project is licensed under the GNU AGPL v3.0 or later. See the [LICENSE](LICENSE) file for details.
