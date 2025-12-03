using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Raven.Client.Documents.Session;

namespace RavenDB.Samples.Library.App;

/// <summary>
/// Powerful caching capabilities based on RavenDB understanding of ETags.
/// </summary>
public static class HttpCaching
{
    public static IActionResult TryCachePublicly(this HttpRequest req, ActionResult original, QueryStatistics stats)
    {
        var ifNoneMatch = req.Headers.IfNoneMatch;

        if (stats.ResultEtag == null)
        {
            // No etag, return the original without any ETag
            return original;
        }

        var etag = stats.ResultEtag.ToString();
        if (ifNoneMatch.Count == 0 || ifNoneMatch.First() != etag)
        {
            // Request contained no ETag or was different, but the resource has one. ETag it!
            return new ETaggedResult(original, etag);
        }

        // Not modified
        return new NotModifiedResult(etag);
    }

    private static readonly StringValues CachePublic = new("public");
    
    private sealed class NotModifiedResult(string etag) :
        ActionResult,
        IClientErrorActionResult
    {
        public override void ExecuteResult(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var ctx = context.HttpContext;
            ctx.Response.StatusCode = StatusCodes.Status304NotModified;
            ctx.Response.Headers.ETag = etag;
            ctx.Response.Headers.CacheControl = CachePublic;
        }

        public int? StatusCode => StatusCodes.Status304NotModified;
    }

    private sealed class ETaggedResult(ActionResult original, string etag) :
        ActionResult
    {
        public override void ExecuteResult(ActionContext context)
        {
            MakeCacheable(context);

            // Execute the original
            original.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            MakeCacheable(context);
            
            // Execute the original
            return original.ExecuteResultAsync(context);
        }

        private void MakeCacheable(ActionContext context)
        {
            // Set caching and status
            var ctx = context.HttpContext;
            ctx.Response.Headers.ETag = etag;
            ctx.Response.Headers.CacheControl = CachePublic;
        }
    }
}