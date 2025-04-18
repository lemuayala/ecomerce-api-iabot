using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Http.Headers;

public class AzureOpenAIHealthCheck : IHealthCheck
{
    private readonly string _endpoint;
    private readonly string _apiKey;

    public AzureOpenAIHealthCheck(IConfiguration config)
    {
        _endpoint = config["AZURE_OPENAI_ENDPOINT"];
        _apiKey = config["AZURE_OPENAI_APIKEY"];
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("api-key", _apiKey);

            var response = await client.GetAsync(
                $"{_endpoint.TrimEnd('/')}/openai/deployments?api-version=2023-05-15",
                cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Degraded($"Status code: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}