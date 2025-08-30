using System.Net;
using System.Text;
using System.Text.Json;

namespace FluentHttp;

public static class HttpListenerRequestExtensions
{
    public static async Task<string> ReadBodyAsync(this HttpListenerRequest request, Encoding? encoding = null, CancellationToken cancel = default)
    {
        encoding = request.ContentEncoding ?? Encoding.UTF8;

        using var reader = new StreamReader(request.InputStream, encoding, leaveOpen: true);
        string body = await reader.ReadToEndAsync(cancel);

        if (request.InputStream.CanSeek)
            request.InputStream.Position = 0;

        return body;
    }

    public static async Task<T?> ReadJsonAsync<T>(this HttpListenerRequest request, JsonSerializerOptions? options = null, Encoding? encoding = null, CancellationToken cancel = default)
    {
        var body = await request.ReadBodyAsync(encoding, cancel);

        if (string.IsNullOrWhiteSpace(body))
            return default;

        return JsonSerializer.Deserialize<T>(body, options);
    }
}