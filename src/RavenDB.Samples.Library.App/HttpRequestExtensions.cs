using Microsoft.AspNetCore.Http;

namespace RavenDB.Samples.Library.App;

public static class HttpRequestExtensions
{
    public static int GetQueryInt(this HttpRequest req, string name, int defaultValue, int min, int max) =>
        req.Query.TryGetValue(name, out var value) && int.TryParse(value, out var parsed)
            ? Math.Clamp(parsed, min, max)
            : defaultValue;

    public static string? GetQueryString(this HttpRequest req, string name) =>
        req.Query.TryGetValue(name, out var value) ? value.ToString() : null;
}
