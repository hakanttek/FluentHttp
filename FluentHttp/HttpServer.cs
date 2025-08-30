using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FluentHttp;

public class HttpServer
{
    private readonly HttpListener _listener;
    private readonly ILogger<HttpServer>? _logger;

    public HttpServer(ILogger<HttpServer>? logger = null)
    {
        _listener = new HttpListener();
        _logger = logger;
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _logger?.LogInformation("HTTP Server started at {Prefix}", string.Join(", ", _listener.Prefixes));

        while (true)
        {
            try
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                _ = ProcessRequestAsync(context); // fire & forget
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An unexpected error occurred while processing a request.");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        _logger?.LogInformation("Incoming request: {Method} {Url} from {RemoteEndPoint}",
            request.HttpMethod, request.Url, request.RemoteEndPoint);

        string responseString = $"<html><body><h1>Hello from SimpleHttpServer at {DateTime.Now}</h1></body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();

        _logger?.LogInformation("Response sent successfully with {Length} bytes.", buffer.Length);
    }

    public HttpServer ListenOn(params string[] urls)
    {
        foreach (var url in urls)
            _listener.Prefixes.Add(url);
        return this;
    }

    public HttpServer ListenOn(params int[] ports)
    {
        foreach (var port in ports)
            _listener.Prefixes.Add($"http://localhost:{port}/");
        return this;
    }
}
