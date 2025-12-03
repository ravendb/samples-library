using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
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
    extension(HttpRequest req)
    {
        public IActionResult TryCachePublicly(ActionResult original, QueryStatistics stats)
        {
            if (stats.ResultEtag == null)
            {
                // No etag, return the original without any ETag
                return original;
            }

            return TryCachePublicly(req, original, stats.ResultEtag.GetValueOrDefault(0).ToString());
        }

        public IActionResult TryCachePublicly<T>(ActionResult original, QueryStatistics stats, IAsyncDocumentSession session, T entity)
        {
            var vector = session.Advanced.GetChangeVectorFor(entity);
        
            if (stats.ResultEtag == null && vector == null)
            {
                // No etag, return the original without any ETag
                return original;
            }

            return TryCachePublicly(req, original, stats.ResultEtag.GetValueOrDefault(0).ToString(), vector);
        }
        
        public IActionResult TryCachePublicly<T1, T2>(ActionResult original, IAsyncDocumentSession session, T1 entity1, T2 entity2)
        {
            var v1 = session.Advanced.GetChangeVectorFor(entity1);
            var v2 = session.Advanced.GetChangeVectorFor(entity2);

            if (v1 == null || v2 == null)
            {
                // No etag, return the original without any ETag
                return original;
            }

            return TryCachePublicly(req, original, v1, v2);
        }
    }

    private static IActionResult TryCachePublicly(HttpRequest req, ActionResult original, params Span<string> parts)
    {
        var etag = CreateETag(parts);

        var ifNoneMatch = req.Headers.IfNoneMatch;
        if (ifNoneMatch.Count == 0 || ifNoneMatch.First() != etag)
        {
            // Request contained no ETag or was different, but the resource has one. ETag it!
            return new ETaggedResult(original, etag);
        }

        // Not modified
        return new NotModifiedResult(etag);
    }

    private static string CreateETag(Span<string> parts)
    {
        return string.Join("=", parts);
    }

    private static readonly StringValues CachePublic = new("public, must-revalidate");
    
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