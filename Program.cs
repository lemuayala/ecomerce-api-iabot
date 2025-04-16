using dotenv.net;
using Microsoft.SemanticKernel;

DotEnv.Load();

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_APIKEY");
var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: deployment,
        endpoint: endpoint,
        apiKey: apiKey)
    .Build();

app.MapGet("/", () => "API de E-Commerce con IA");

app.Run();