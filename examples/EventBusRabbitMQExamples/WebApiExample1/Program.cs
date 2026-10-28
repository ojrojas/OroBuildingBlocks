using BuildingBlocks.EventBus.RabbitMQ.DependencyInjection;
using WebApiExample1.IntegrateEvents;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddRabbitMqEventBus(builder.Configuration, options =>
{
    options.ExchangeName = "oroeventdrivenexchange";
    options.QueueName = "webapi1-queue";
}).AddSubscription<ExampleIntegrateEvents, ExampleIntegrateEventsHandler>();

WebApplication app = builder.Build();

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
