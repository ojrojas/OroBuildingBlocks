using BuildingBlocks.EventBus.Abstractions;
using BuildingBlocks.EventBus.RabbitMQ.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using WebApiExample2.IntegrationEvents;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRabbitMqEventBus(builder.Configuration, options =>
{
    options.ExchangeName = "oroeventdrivenexchange";
});

WebApplication app = builder.Build();

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
