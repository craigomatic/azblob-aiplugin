using System.Net;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

public class PluginEndpoint
{
    private readonly ILogger _logger;
    private readonly Azure.Storage.Blobs.BlobServiceClient _blobServiceClient;
    private readonly BlobConfig _blobConfig;

    public PluginEndpoint(ILoggerFactory loggerFactory, Azure.Storage.Blobs.BlobServiceClient blobServiceClient, BlobConfig blobConfig)
    {
        _logger = loggerFactory.CreateLogger<PluginEndpoint>();
        _blobServiceClient = blobServiceClient;
        _blobConfig = blobConfig;
    }

    [Function("WellKnownAIPlugin")]
    public async Task<HttpResponseData> WellKnownAIPlugin(
     [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = ".well-known/ai-plugin.json")] HttpRequestData req)
    {
        var toReturn = new AIPlugin();
        toReturn.Api.Url = $"{req.Url.Scheme}://{req.Url.Host}:{req.Url.Port}/swagger.json";

        var r = req.CreateResponse(HttpStatusCode.OK);
        await r.WriteAsJsonAsync(toReturn);
        return r;
    }

    [OpenApiOperation(operationId: "CreateBlockBlob", tags: new[] { "CreateBlockBlob" }, Description = "Creates a block blob with a random file name.")]
    [OpenApiParameter(name: "TTL", Description = "The amount of time in minutes that the blob should be accessible via a Shared Access Signature", Required = true, In = ParameterLocation.Query)]
    [OpenApiParameter(name: "Extension", Description = "An optional file extension for the created blob filename", Required = false, In = ParameterLocation.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "text/plain", bodyType: typeof(string), Description = "Confirms that a block blob was created and returns a writeable URI to it.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
    [Function("CreateBlockBlob")]
    public async Task<HttpResponseData> CreateBlockBlob([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Beginning to create a block blob");

        if (!_TryValidateTtl(req, out var ttl))
        {
            var r = req.CreateResponse(HttpStatusCode.BadRequest);
            r.Headers.Add("Content-Type", "application/json; charset=utf-8");
            r.WriteString("TTL is required.");
            return r;
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobConfig.ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlockBlobClient(blobName: _GetBlobNameWithExtension(req.Query("Extension").FirstOrDefault()));
        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create, DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(ttl)));

        _logger.LogInformation("Block blob SAS URI created");

        var r2 = req.CreateResponse(HttpStatusCode.Created);
        r2.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        r2.WriteString(sasUri.ToString());
        return r2;
    }

    [OpenApiOperation(operationId: "CreateAppendBlob", tags: new[] { "CreateAppendBlob" }, Description = "Creates an appendable blob with a random file name.")]
    [OpenApiParameter(name: "TTL", Description = "The amount of time in minutes that the append blob should be accessible via a Shared Access Signature", Required = true, In = ParameterLocation.Query)]
    [OpenApiParameter(name: "Extension", Description = "An optional file extension for the created blob filename", Required = false, In = ParameterLocation.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "text/plain", bodyType: typeof(string), Description = "Confirms that an append blob was created and returns a writeable URI to it.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
    [Function("CreateAppendBlob")]
    public async Task<HttpResponseData> CreateAppendBlob([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Beginning to create an append blob");

        if (!_TryValidateTtl(req, out var ttl))
        {
            var r = req.CreateResponse(HttpStatusCode.BadRequest);
            r.Headers.Add("Content-Type", "application/json; charset=utf-8");
            r.WriteString("TTL is required.");
            return r;
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobConfig.ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetAppendBlobClient(blobName: _GetBlobNameWithExtension(req.Query("Extension").FirstOrDefault()));
        await blobClient.CreateAsync();
        
        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create, DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(ttl)));

        _logger.LogInformation("Append blob SAS URI created");

        var r2 = req.CreateResponse(HttpStatusCode.Created);
        r2.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        r2.WriteString(sasUri.ToString());
        return r2;
    }

    [OpenApiOperation(operationId: "CreatePageBlob", tags: new[] { "CreatePageBlob" }, Description = "Creates a page blob with a random file name.")]
    [OpenApiParameter(name: "TTL", Description = "The amount of time in minutes that the page blob should be accessible via a Shared Access Signature", Required = true, In = ParameterLocation.Query)]
    [OpenApiParameter(name: "Extension", Description = "An optional file extension for the created blob filename", Required = false, In = ParameterLocation.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "text/plain", bodyType: typeof(string), Description = "Confirms that a page blob was created and returns a writeable URI to it.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
    [Function("CreatePageBlob")]
    public async Task<HttpResponseData> CreatePageBlob([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Beginning to create a page blob");

        if (!_TryValidateTtl(req, out var ttl))
        {
            var r = req.CreateResponse(HttpStatusCode.BadRequest);
            r.Headers.Add("Content-Type", "application/json; charset=utf-8");
            r.WriteString("TTL is required.");
            return r;
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_blobConfig.ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetPageBlobClient(blobName: _GetBlobNameWithExtension(req.Query("Extension").FirstOrDefault()));

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Read | BlobSasPermissions.Write | BlobSasPermissions.Create, DateTimeOffset.UtcNow.AddMinutes(Convert.ToDouble(ttl)));

        _logger.LogInformation("Page blob SAS URI created");

        var r2 = req.CreateResponse(HttpStatusCode.Created);
        r2.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        r2.WriteString(sasUri.ToString());
        return r2;
    }

    private string _GetBlobNameWithExtension(string? fileExtension)
    {
        return fileExtension == null ?
            Guid.NewGuid().ToString() : 
            $"{Guid.NewGuid()}.{fileExtension.TrimStart('.')}";
    }

    private bool _TryValidateTtl(HttpRequestData req, out string? ttl)
    {
        ttl = req.Query("TTL").FirstOrDefault();

        if (ttl == null)
        {
            _logger.LogInformation("Missing TTL, aborting");

            return false;
        }

        return true;
    }
}
