using System.Net;

namespace FluentHttp.Models;

public record HttpResult(int StatusCode, object? Data = null)
{
    public HttpResult(HttpStatusCode statusCode, object? data = null)
        : this((int)statusCode, data)
    {
    }
}