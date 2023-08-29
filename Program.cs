using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Text.Json;

var builtConfig = null as IConfigurationRoot;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(defaults =>
    {
        defaults.Serializer = new Azure.Core.Serialization.JsonObjectSerializer(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });
    })
    .ConfigureAppConfiguration(configuration =>
    {
        var config = configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

        builtConfig = config.Build();

    })
    .ConfigureServices(services =>
    {
        var appConfig = builtConfig.Get<AppConfig>();
        
        services.AddSingleton(_ => new BlobServiceClient(appConfig.StorageConnectionString));
        services.AddSingleton(_ => new BlobConfig { ContainerName = appConfig.ContainerName} );

        services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
        {
            var options = new OpenApiConfigurationOptions()
            {
                Info = new OpenApiInfo()
                {
                    Version = "1.0.0",
                    Title = "Azure Blob Storage Plugin",
                    Description = "This plugin is capable of creating blobs that can be written to and read from."
                },
                Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                OpenApiVersion = OpenApiVersionType.V3,
                IncludeRequestingHostName = true,
                ForceHttps = false,
                ForceHttp = false,
            };

            return options;
        });
    })
    .Build();

host.Run();