using GrpcUserService.Grpc.Extensions;
using InfrastructureUserService.Infrastructure.Extensions;
using KafkaUserService.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddGrpcServices();

builder.Services.AddKafkaEventPublisher(builder.Configuration);

IConfigurationSection kestrelSection = builder.Configuration.GetSection("Kestrel");
if (kestrelSection.Exists())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Configure(kestrelSection);
    });
}
else
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(
            5000,
            listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });

        options.ListenAnyIP(
            5001,
            listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
            });
    });
}

WebApplication app = builder.Build();

app.MapGrpcService<GrpcUserService.Grpc.Controllers.UserGrpcService>();

app.MapGet("/", () => "User Service gRPC is running");

app.Run();