IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<RabbitMQServerResource> rabbitMQ = builder.AddRabbitMQ("oroeventdrivenexchange")

// .WithLifetime(ContainerLifetime.Persistent);
;
IResourceBuilder<ProjectResource> webapi1 = builder.AddProject<Projects.WebApiExample1>("WebApi1");
IResourceBuilder<ProjectResource> webapi2 = builder.AddProject<Projects.WebApiExample2>("WebApi2");

webapi1.WithReference(rabbitMQ).WaitFor(rabbitMQ);
webapi2.WithReference(rabbitMQ).WaitFor(rabbitMQ);

builder.Build().Run();
