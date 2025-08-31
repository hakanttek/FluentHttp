using System.Net;

namespace FluentHttp.Models;

public record HttpResult(int StatusCode, object? Data = null);

public static class Extensions
{
    public static HttpResult Data(this HttpStatusCode statusCode, object data) => new ((int)statusCode, data);
}