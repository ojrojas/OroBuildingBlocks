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

### 🚌 Event Bus (RabbitMQ)
The `EventBus` and `EventBusRabbitMQ` projects provide a robust infrastructure for asynchronous messaging:
- **Integration Events**: Core abstractions for defining and handling cross-service events.
- **RabbitMQ Implementation**: High-performance, async-native implementation optimized for RabbitMQ.Client 7.0 and .NET 10.
- **Resilience**: Integrated retry logic using Polly Resilience Pipelines to handle transient broker connection issues.
- **Observability**: Native OpenTelemetry integration for distributed tracing, including automatic context propagation across producers and consumers.

### �📝 Logging
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
dotnet add reference src/EventBusRabbitMQ/EventBusRabbitMQ.csproj
```

In your `Program.cs`, you can then use the extensions:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add standard service defaults
builder.AddServiceDefaults();

// Configure logging
var logger = LoggerPrinter.CreateSerilogLogger("AppName", "MyService", builder.Configuration);
builder.Host.UseSerilog(logger);

// Register RabbitMQ Event Bus
builder.Services.AddRabbitMQEventBus("EventBusConnection")
    .AddSubscription<MyIntegrationEvent, MyIntegrationEventHandler>();

var app = builder.Build();

// Map standard endpoints
app.MapDefaultEndpoints();

app.Run();
```

## License

This project is licensed under the GNU AGPL v3.0 or later. See the [LICENSE](LICENSE) file for details.
