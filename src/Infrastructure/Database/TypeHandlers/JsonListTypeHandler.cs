using System.Data;
using Dapper;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace EcomerceAI.Api.Infrastructure.Database.TypeHandlers;

public class JsonListTypeHandler<T> : SqlMapper.TypeHandler<T> where T : class
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true
    };

    public override T Parse(object value)
    {
        if (value == null || value is DBNull)
            return default;

        var json = value.ToString();
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? default;
        }
        catch (JsonException)
        {
            return default;
        }
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        parameter.Value = value == null
            ? DBNull.Value
            : JsonSerializer.Serialize(value, JsonOptions);
    }
}