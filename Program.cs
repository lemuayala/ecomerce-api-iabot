using dotenv.net;
using Microsoft.Data.SqlClient;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configuración de Azure OpenAI
var aiConfig = new
{
    Endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
    ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_APIKEY"),
    DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
    EmbeddingDeployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT")
};

// Configuración de la base de datos
var dbConfig = new
{
    SqlConnectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING"),
    MongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING")
};

// Configuración del kernel de IA
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: aiConfig.DeploymentName,
        endpoint: aiConfig.Endpoint,
        apiKey: aiConfig.ApiKey)
    .Build();

// Configuración de logging
builder.Services.AddLogging(configure =>
    configure.AddConsole().AddDebug());

// Configuración de Health Checks
var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddCheck<AzureOpenAIHealthCheck>("azure-openai");

if (!string.IsNullOrEmpty(dbConfig.SqlConnectionString))
{
    healthChecksBuilder.AddSqlServer(
        connectionString: dbConfig.SqlConnectionString,
        name: "sql-server",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "db", "sql" });
}

// if (!string.IsNullOrEmpty(dbConfig.MongoConnectionString))
// {
//     healthChecksBuilder.AddMongoDb(
//         name: "mongodb",
//         failureStatus: HealthStatus.Degraded,
//         tags: new[] { "db", "nosql" });
// }

// Registrar servicios
builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton(aiConfig);
builder.Services.AddSingleton(dbConfig);

// Configuración de la conexión SQL
builder.Services.AddScoped(_ =>
    new SqlConnection(dbConfig.SqlConnectionString));

// Repositorios
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Servicios de IA
builder.Services.AddScoped<IRecommendationService, AIRecommendationService>();

var app = builder.Build();

// Configuración del endpoint de health checks
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

app.MapGet("/", () => "E-Commerce AI Assistant API v1.0");

// Grupo de endpoints para productos
var productGroup = app.MapGroup("/api/products");

productGroup.MapGet("/{id}", async (int id, IProductRepository repo) =>
{
    var product = await repo.GetByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

productGroup.MapGet("/recommendations/{userId}",
    async (int userId, IRecommendationService service) =>
    {
        var recommendations = await service.GetPersonalizedRecommendations(userId);
        return Results.Ok(recommendations);
    });

productGroup.MapGet("/search", async (string query, IProductRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest("Query parameter is required");

    var results = await repo.SearchAsync(query);
    return Results.Ok(results);
});

app.Run();