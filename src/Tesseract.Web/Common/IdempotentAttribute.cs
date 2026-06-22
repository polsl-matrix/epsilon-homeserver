using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tesseract.Application.ClientServer.Common.Repositories;
using Tesseract.Web.Common.Auth;

namespace Tesseract.Web.Common;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotentAttribute()
    : TypeFilterAttribute(typeof(IdempotentFilter));

public class IdempotentFilter(ICurrentUser user, IResponseRepository responseRepository)
    : IAsyncActionFilter, IAsyncResultFilter
{
    private static readonly object CacheRestoreFlag = new();

    public async Task OnActionExecutionAsync(
        ActionExecutingContext acctionExecutingContext, ActionExecutionDelegate next)
    {
        var context = acctionExecutingContext.HttpContext;
        var cancellationToken = context.RequestAborted;

        var path = context.Request.Path;

        if (await GetCachedResponseAsync(path, cancellationToken) is { } response)
        {
            WriteCachedResponse(acctionExecutingContext, response);
            SetRestoreFlag(context);
            return;
        }

        await next();
    }

    public async Task OnResultExecutionAsync(
        ResultExecutingContext resultExecutingContext, ResultExecutionDelegate next)
    {
        var context = resultExecutingContext.HttpContext;

        if (HasRestoreFlag(context))
        {
            await next();
            return;
        }

        var response = context.Response;
        var content = await CaptureResponseAsync(context, next);

        if (response.StatusCode is not (>= 200 and <= 299))
        {
            return;
        }

        var path = context.Request.Path;

        await SaveCachedResponseAsync(path, response, content, context.RequestAborted);
    }

    private static async Task<string> CaptureResponseAsync(HttpContext context, ResultExecutionDelegate next)
    {
        var cancellationToken = context.RequestAborted;

        await using var inMemoryBodyStream = new MemoryStream();

        var originalBodyStream = context.Response.Body;
        context.Response.Body = inMemoryBodyStream;

        try
        {
            await next();

            inMemoryBodyStream.Position = 0;
            await inMemoryBodyStream.CopyToAsync(originalBodyStream, cancellationToken);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }

        inMemoryBodyStream.Position = 0;

        using var reader = new StreamReader(inMemoryBodyStream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static bool HasRestoreFlag(HttpContext context) =>
        context.Items.TryGetValue(CacheRestoreFlag, out var flag) && flag is true;

    private static void SetRestoreFlag(HttpContext context) =>
        context.Items[CacheRestoreFlag] = true;

    private static void WriteCachedResponse(ActionExecutingContext context, Response response) =>
        context.Result = new ContentResult
        {
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            Content = response.Content,
        };

    private Task<Response?> GetCachedResponseAsync(string path, CancellationToken cancellationToken) =>
        responseRepository.GetByUserIdAndPathAsync(user.Id, path, cancellationToken);

    private Task SaveCachedResponseAsync(string path, HttpResponse response, string content,
        CancellationToken cancellationToken) =>
        responseRepository.UpsertAsync(new Response
        {
            UserId = user.Id,
            Path = path,
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            Content = content,
        }, cancellationToken);
}