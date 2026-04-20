using Microsoft.AspNetCore.Mvc;
using OroBuildingBlocks.EventBus.Abstractions;
using OroBuildingBlocks.EventBusRabbitMQ;
using WebApiExample2.IntegrationEvents;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddRabbitMqEventBus("oroeventdrivenexchange");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/message", async ([FromQuery] string message, IEventBus eventBus, CancellationToken cancellationToken) =>
{
    var integrationEvent = new ExampleIntegrateEvents(message);
    await eventBus.PublishAsync(integrationEvent, cancellationToken);
    return Results.Ok("Message sent to the event bus");

})
.WithName("SendMessageEvent");

app.Run();
