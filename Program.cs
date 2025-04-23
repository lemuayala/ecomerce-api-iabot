using dotenv.net;
using Microsoft.Data.SqlClient;
using Microsoft.SemanticKernel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using EcomerceAI.Api.Infrastructure.Database.TypeHandlers;
using Dapper;
using System.Text.Encodings.Web;
using Azure.Storage.Blobs;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configuración básica
builder.Services
    .AddLogging(configure => configure.AddConsole().AddDebug())
    .AddHttpContextAccessor();

// Registrar los Type Handlers de Dapper
SqlMapper.AddTypeHandler(new JsonListTypeHandler<List<string>>());
SqlMapper.AddTypeHandler(new JsonDictTypeHandler<Dictionary<string, string>>());

// Configuración de Azure OpenAI
var aiConfig = new
{
    Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
    ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_APIKEY"),
    DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT")
};

// Configurar Semantic Kernel
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: aiConfig.DeploymentName,
        endpoint: aiConfig.Endpoint,
        apiKey: aiConfig.ApiKey)
    .Build();

builder.Services.AddSingleton(kernel);

// Configuración de Azure Blob Storage
builder.Services.AddSingleton<BlobServiceClient>(provider =>
{
    var connectionString = Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_STRING");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("La cadena de conexión de Azure Blob Storage ('AZURE_BLOB_STORAGE_STRING') no está configurada.");
    }

    return new BlobServiceClient(
        connectionString,
        new BlobClientOptions
        {
            Retry = {
                MaxRetries = 3,
                Mode = Azure.Core.RetryMode.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(10)
            }
        }
    );
});

// Configuración de la base de datos
var dbConfig = new
{
    SqlConnectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING")
};

// Registrar servicios de infraestructura
builder.Services
    .AddScoped(_ => new SqlConnection(dbConfig.SqlConnectionString))
    .AddScoped<IProductRepository, ProductRepository>()
    .AddScoped<IRecommendationService, RecommendationService>();

// Configuración de MediatR (CQRS)
// builder.Services.AddMediatR(cfg =>
//     cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(dbConfig.SqlConnectionString, name: "sql-server")
    .AddCheck<AzureOpenAIHealthCheck>("azure-openai");

// Configurar las opciones de serialización JSON para toda la aplicación
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.PropertyNameCaseInsensitive = true;
});

builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();

var app = builder.Build();

// Middleware básico
app.UseHttpsRedirection();

// Health Check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                exception = e.Value.Exception?.Message,
                duration = e.Value.Duration.ToString()
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Endpoints
app.MapGet("/", () => "E-Commerce AI Assistant API v1.0");
app.MapProductEndpoints();

app.Run();