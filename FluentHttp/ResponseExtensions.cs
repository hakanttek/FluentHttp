using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FluentHttp;

public static class HttpListenerRequestExtensions
{
    public static async Task<string> TextAsync(this HttpListenerRequest request, Encoding? defaultEncoding = null, CancellationToken cancel = default)
    {
        var encoding = request.ContentEncoding ?? defaultEncoding ?? Encoding.UTF8;

        using var reader = new StreamReader(request.InputStream, encoding, leaveOpen: true);
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

    public static async Task WriteAsync(this HttpListenerResponse response, string text, Encoding? defaultEncoding = null, string? contentType = null, CancellationToken cancel = default)
    {
        var encoding = response.ContentEncoding ?? defaultEncoding ?? Encoding.UTF8;
        response.ContentType = contentType ?? $"text/plain; charset={encoding.WebName}";

        byte[] buffer = encoding.GetBytes(text);
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer, cancel);
        await response.OutputStream.FlushAsync(cancel);
    }
}