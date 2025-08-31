using System.Net;

namespace FluentHttp.Models;

public static class HttpResultExtensions
{
    public static HttpResult Data(this HttpStatusCode statusCode, object? data) => new((int)statusCode, data);

    internal static async Task<HttpResult?> NormalizeToHttpResultAsync(this object? output)
    {
        switch (output)
        {
            case HttpStatusCode code:
                return new HttpResult(code);

            case Task<HttpStatusCode> codeTask:
                return new HttpResult(await codeTask);

            case HttpResult result:
                return result;

            case Task<HttpResult> resultTask:
                return await resultTask;

            case Task task:
                await task;
                return null;

            default:
                throw new NotSupportedException(
                    $"The provided output type '{output?.GetType().FullName ?? "null"}' " +
                    "is not supported. Expected types are: " +
                    "HttpStatusCode, Task<HttpStatusCode>, HttpResult, Task<HttpResult>, or Task."
                );
        }
    }
}