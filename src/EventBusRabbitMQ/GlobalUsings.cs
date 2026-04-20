// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
global using System.Text;
global using System.Text.Json;
global using OroBuildingBlocks.EventBus.Abstractions;
global using OroBuildingBlocks.EventBus.Events;
global using System.Diagnostics;
global using Microsoft.Extensions.Logging;
global using OpenTelemetry.Context.Propagation;
global using Microsoft.Extensions.Hosting;
global using System.Diagnostics.CodeAnalysis;
global using System.Net.Sockets;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using OpenTelemetry;
global using Polly;
global using Polly.Retry;
global using RabbitMQ.Client;
global using RabbitMQ.Client.Events;
global using RabbitMQ.Client.Exceptions;