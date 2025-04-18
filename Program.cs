using dotenv.net;
using Microsoft.Data.SqlClient;
using Microsoft.SemanticKernel;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using MediatR;
using EcomerceAI.Api.Features.Products.Application.Queries;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configuración básica
builder.Services
    .AddLogging(configure => configure.AddConsole().AddDebug())
    .AddHttpContextAccessor();

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
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(dbConfig.SqlConnectionString, name: "sql-server")
    .AddCheck<AzureOpenAIHealthCheck>("azure-openai");

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

// Grupo de endpoints para productos
var productGroup = app.MapGroup("/api/products");

productGroup.MapGet("/{id}", async (int id, IProductRepository repo) =>
    await repo.GetByIdAsync(id) is { } product
        ? Results.Ok(product)
        : Results.NotFound());

productGroup.MapGet("/cqrs/{id}", async (int id, IMediator mediator) =>
    await mediator.Send(new GetProductByIdQuery(id)) is { } product
        ? Results.Ok(product)
        : Results.NotFound());

// Endpoint de recomendaciones con IA
productGroup.MapGet("/recommendations/{userId}",
    async (int userId, IRecommendationService service) =>
        Results.Ok(await service.GetPersonalizedRecommendations(userId)));

// Endpoint de búsqueda (Repository)
productGroup.MapGet("/search", async (string query, IProductRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest("Query parameter is required");

    return Results.Ok(await repo.SearchAsync(query));
});

app.MapProductEndpoints();

app.Run();