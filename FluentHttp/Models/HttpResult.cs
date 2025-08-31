using System.Net;

namespace FluentHttp.Models;

public record HttpResult(int StatusCode, object? Data = null)
{
    public HttpResult(HttpStatusCode statusCode, object? data = null)
        : this((int)statusCode, data)
    {
    }
}

public static class Extensions
{
    public static HttpResult Data(this HttpStatusCode statusCode, object data) => new ((int)statusCode, data);
}