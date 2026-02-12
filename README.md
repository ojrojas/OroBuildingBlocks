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

# OroBuildingBlocks

OroBuildingBlocks is a set of reusable .NET components and infrastructure "building blocks" to accelerate development of microservices and distributed systems. The repository provides opinionated, small libraries that cover observability, resilience, event-driven messaging and structured logging.

## Current repository structure

- `src/ServiceDefaults/` — Helpers and DI extensions for bootstrapping common service concerns (OpenTelemetry, health checks, service discovery, and HTTP client resilience).
- `src/Loggers/` — Serilog helper(s) and logging utilities (includes Seq sink configuration helper).
- `src/EventBus/` — Core event bus abstractions and integration event model.
- `src/EventBusRabbitMQ/` — RabbitMQ-backed `IEventBus` implementation with publishing and consuming logic, resilience, and telemetry propagation.

### Platforms & packaging

- Target framework: `net10.0` for the library projects.
- Each project contains NuGet package metadata in its `.csproj` (package id, version, authors, and assets).

## Key features (current implementation)

- **ServiceDefaults**
    - OpenTelemetry integration (tracing and metrics) using `OpenTelemetry.*` packages.
    - Automatic configuration to add health checks and default liveness/readiness endpoints via `MapDefaultEndpoints()`.
    - Service discovery support via `Microsoft.Extensions.ServiceDiscovery`.
    - HTTP client resilience using `Microsoft.Extensions.Http.Resilience` and a resilient handler added by default.
    - OTLP exporter detection: when `OTEL_EXPORTER_OTLP_ENDPOINT` is set, the OTLP exporter is enabled.

- **Logging (Loggers)**
    - `LoggerPrinter.CreateSerilogLogger` helper that configures Serilog with Console and Seq sinks.
    - Exposes an easy entry point to create and use a Serilog logger configured from app configuration.

- **Event Bus**
    - `IntegrationEvent` record as the base event model (includes `Id` and `Created`).
    - `IEventBus` abstraction exposing `PublishAsync`.

- **EventBusRabbitMQ**
    - Concrete RabbitMQ implementation using an exchange named `eventdrivendesignbus` and durable queues.
    - Uses a `ResiliencePipeline` (from `Microsoft.Extensions.Http.Resilience`) for retry logic when publishing, with backoff based on attempt number.
    - Full OpenTelemetry support: activities are created for publish/receive, propagation of context into RabbitMQ message headers, and tags for messaging semantics.
    - Background consumer that binds subscriptions to the queue, processes messages, resolves `IIntegrationEventHandler` implementations from DI, and acknowledges messages.
    - JSON serialization using configurable `JsonSerializerOptions` from subscription info.

## Notable differences vs older README text

- The implementation uses `Microsoft.Extensions.Http.Resilience`'s `ResiliencePipeline` rather than a direct Polly dependency.
- OpenTelemetry export configuration is automatic (OTLP enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is present).
- Logging helper actively requires a Seq endpoint from configuration when creating the Serilog logger (`ConnectionStrings:Seq`).

## Quick start

1. Reference the building block projects you need:

```bash
dotnet add reference src/ServiceDefaults/ServiceDefaults.csproj
dotnet add reference src/Loggers/Loggers.csproj
dotnet add reference src/EventBusRabbitMQ/EventBusRabbitMQ.csproj
```

2. Example usage in `Program.cs` (conceptual):

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add standard service defaults (OpenTelemetry, health checks, resilience, discovery)
builder.AddServiceDefaults();

// Configure logging
var logger = LoggerPrinter.CreateSerilogLogger("Application", "MyService", builder.Configuration);
builder.Host.UseSerilog(logger);

// Register the RabbitMQ event bus (project provides DI registration extensions)
// builder.Services.AddRabbitMQEventBus(...);

var app = builder.Build();
app.MapDefaultEndpoints();
app.Run();
```

Adjust the event-bus registration and subscription wiring according to your app's DI setup and configuration values.

## Where to look in the codebase

- Service startup helpers: [src/ServiceDefaults/Extensions.cs](src/ServiceDefaults/Extensions.cs)
- Serilog helper: [src/Loggers/LoggerPrinter.cs](src/Loggers/LoggerPrinter.cs)
- Event model & abstraction: [src/EventBus/Events/IntegrationEvent.cs](src/EventBus/Events/IntegrationEvent.cs), [src/EventBus/Abstractions/IEventBus.cs](src/EventBus/Abstractions/IEventBus.cs)
- RabbitMQ implementation: [src/EventBusRabbitMQ/EventBusRabbitMQ.cs](src/EventBusRabbitMQ/EventBusRabbitMQ.cs)

## License

This project is licensed under the GNU AGPL v3.0 or later. See the [LICENSE](LICENSE) file for details.
