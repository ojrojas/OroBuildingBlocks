var builder = DistributedApplication.CreateBuilder(args);

var rabbitMQ = builder.AddRabbitMQ("oroeventdrivenexchange")

// .WithLifetime(ContainerLifetime.Persistent);
;
var webapi1 = builder.AddProject<Projects.WebApiExample1>("WebApi1");
var webapi2 = builder.AddProject<Projects.WebApiExample2>("WebApi2");

webapi1.WithReference(rabbitMQ).WaitFor(rabbitMQ);
webapi2.WithReference(rabbitMQ).WaitFor(rabbitMQ);

builder.Build().Run();
