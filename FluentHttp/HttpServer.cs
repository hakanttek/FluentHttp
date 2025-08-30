using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace FluentHttp;

public partial class HttpServer(HttpListener listener, ILogger<HttpServer>? logger = null)
{
    public async Task StartAsync()
    {
        listener.Start();
        logger?.LogInformation("HTTP Server started at {Prefix}", string.Join(", ", listener.Prefixes));

        while (true)
        {
            try
            {
                HttpListenerContext context = await listener.GetContextAsync();
                _ = ProcessRequestAsync(context); // fire & forget
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An unexpected error occurred while processing a request.");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context, CancellationToken cancel = default)
    {
        var request = context.Request;
        var response = context.Response;
        
        logger?.LogInformation("Incoming request: {Method} {Url} from {RemoteEndPoint}",
            request.HttpMethod, request.Url, request.RemoteEndPoint);

        logger?.LogInformation("Route: {route}", request?.Url?.AbsolutePath);
        
        string responseString = $"<html><body><h1>Hello from SimpleHttpServer at {DateTime.Now}</h1></body></html>";
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer, cancel);
        response.OutputStream.Close();

        logger?.LogInformation("Response sent successfully with {Length} bytes.", buffer.Length);
    }  

    private readonly ConcurrentDictionary<(string Method, string path), RequestHandler> EndPoints = new ();

    public HttpServer EndPoint(string method, string path, RequestHandler handler)
    {
        EndPoints[(method.ToUpperInvariant(), path)] = handler;
        return this;
    }

    public HttpServer ListenOn(params string[] urls)
    {
        foreach (var url in urls)
            listener.Prefixes.Add(url);
        return this;
    }

    public HttpServer ListenOn(params int[] ports)
    {
        foreach (var port in ports)
            listener.Prefixes.Add($"http://localhost:{port}/");
        return this;
    }
}
