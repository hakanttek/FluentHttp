using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using FluentHttp.Models;

namespace FluentHttp;

public partial class HttpServer(HttpListener listener, IServiceProvider provider, ILogger<HttpServer>? logger = null)
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
        var startTime = DateTime.UtcNow;

        var request = context.Request;
        var response = context.Response;

        var route = request.Url?.AbsolutePath;
        var query = request.Url?.Query;

        logger?.LogInformation(
            "HTTP {Method} request received\n" +
            "URL        : {FullUrl}\n" +
            "Route      : {Route}\n" +
            "Query      : {Query}\n" +
            "Remote IP  : {RemoteEndPoint}\n" +
            "User Agent : {UserAgent}",
            request.HttpMethod,
            request.Url,
            route,
            string.IsNullOrEmpty(query) ? "-" : query,
            request.RemoteEndPoint,
            request.UserAgent
        );

        HttpResult? handlerRes;
        string method = request.HttpMethod.ToUpperInvariant();
        if (EndPoints.TryGetValue((method, route ?? @"\"), out var handler))
        {
            handlerRes = await provider.InvokeAsHttpResultAsync(handler, request, response, context.User, cancel);
        }
        else
            handlerRes = await provider.InvokeAsHttpResultAsync(_fallback, request, response, context.User, cancel);

        if(handlerRes is not null)
        {
            response.StatusCode = handlerRes.StatusCode;
            if (handlerRes.Data is not null)
                await response.JsonAsync(handlerRes.Data, cancel: cancel);
        }

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        logger?.LogInformation(
            "HTTP response sent successfully\n" +
            "Status Code    : {StatusCode}\n" +
            "Content Length : {ContentLength} bytes\n" +
            "Elapsed Time   : {ElapsedMilliseconds} ms",
            response.StatusCode,
            response.ContentLength64,
            elapsedMs
        );
    }

    private readonly ConcurrentDictionary<(string Method, string path), Delegate> EndPoints = new ();

    public HttpServer EndPoint(string method, string path, Delegate handler)
    {
        EndPoints[(method.ToUpperInvariant(), path)] = handler;
        return this;
    }

    private Delegate _fallback = () => HttpStatusCode.NotFound;

    public HttpServer Fallback(Delegate handler)
    {
        _fallback = handler;
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
