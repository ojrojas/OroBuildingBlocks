using Microsoft.AspNetCore.Mvc;
using OroBuildingBlocks.EventBus.Abstractions;
using OroBuildingBlocks.EventBus.Extensions;
using OroBuildingBlocks.EventBusRabbitMQ;
using WebApiExample1.IntegrateEvents;

var builder = WebApplication.CreateBuilder(args);


ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

   builder.AddRabbitMqEventBus("oroeventdrivenexchange")
               .AddSubscriptionManager<ExampleIntegrateEvents, ExampleIntegrateEventsHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/Info", (CancellationToken cancellationToken) =>
{
    return Results.Ok("return message from WebApiExample1");
})
.WithName("ReturnInfo");

app.Run();