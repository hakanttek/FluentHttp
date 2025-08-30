using System.Net;
using System.Text;
using System.Text.Json;

namespace FluentHttp;

public static class HttpListenerRequestExtensions
{
    public static async Task<string> TextAsync(this HttpListenerRequest request, Encoding? defaultEncoding = null, CancellationToken cancel = default)
    {
        defaultEncoding = request.ContentEncoding ?? defaultEncoding ?? Encoding.UTF8;

        using var reader = new StreamReader(request.InputStream, defaultEncoding, leaveOpen: true);
        string body = await reader.ReadToEndAsync(cancel);

        if (request.InputStream.CanSeek)
            request.InputStream.Position = 0;

        return body;
    }

    public static async Task<T?> JsonAsync<T>(this HttpListenerRequest request, JsonSerializerOptions? options = null, Encoding? defaultEncoding = null, CancellationToken cancel = default)
    {
        var body = await request.TextAsync(defaultEncoding, cancel);

        if (string.IsNullOrWhiteSpace(body))
            return default;

        return JsonSerializer.Deserialize<T>(body, options);
    }
}