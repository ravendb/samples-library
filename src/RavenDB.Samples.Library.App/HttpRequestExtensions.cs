using Microsoft.AspNetCore.Http;

namespace RavenDB.Samples.Library.App;

public static class HttpRequestExtensions
{
    extension(HttpRequest req)
    {
        public int GetQueryInt(string name, int defaultValue, int min, int max) =>
            req.Query.TryGetValue(name, out var value) && int.TryParse(value, out var parsed)
                ? Math.Clamp(parsed, min, max)
                : defaultValue;

        public string? GetQueryString(string name) =>
            req.Query.TryGetValue(name, out var value) ? value.ToString() : null;

    }
}