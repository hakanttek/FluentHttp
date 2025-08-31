using FluentHttp.Attributes;
using FluentHttp.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

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

        string method = request.HttpMethod.ToUpperInvariant();
        EndPoints.TryGetValue((method, route ?? @"\"), out var handler);
        var handlerRes = await InvokeAsHttpResultAsync(handler, request, response, context.User, cancel);

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

    #region JSON Serialization Options
    private JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public HttpServer JsonSerializerOptions(Action<JsonSerializerOptions> configure)
    {
        configure.Invoke(_jsonSerializerOptions);
        return this;
    }

    public HttpServer JsonSerializerOptions(JsonSerializerOptions options)
    {
        _jsonSerializerOptions = options;
        return this;
    }
    #endregion

    #region Body Encoding
    private Encoding _bodyEncoding = Encoding.UTF8;

    public HttpServer BodyEncoding(Encoding encoding)
    {
        _bodyEncoding = encoding;
        return this;
    }
    #endregion

    internal async Task<HttpResult?> InvokeAsHttpResultAsync(
    Delegate? handler,
    HttpListenerRequest request,
    HttpListenerResponse response,
    IPrincipal? user,
    CancellationToken cancel = default)
    {
        handler ??= _fallback;
        var method = handler.Method;
        var parameters = method.GetParameters();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var paramType = param.ParameterType;

            // ---- Attribute-based binding ----
            if (param.GetCustomAttribute<BodyAttribute>() is not null)
                args[i] = await request.JsonAsync(paramType, _jsonSerializerOptions, _bodyEncoding, cancel);
            else if (param.GetCustomAttribute<QueryAttribute>() is { } queryAttr)
            {
                var queryParams = System.Web.HttpUtility.ParseQueryString(request.Url?.Query ?? string.Empty);
                var name = queryAttr.Name ?? param.Name!;
                var value = queryParams[name];
                args[i] = ConvertTo(value, paramType);
            }
            else if (param.GetCustomAttribute<HeaderAttribute>() is { } headerAttr)
            {
                var name = headerAttr.Name ?? param.Name!;
                var value = request.Headers[name];
                args[i] = ConvertTo(value, paramType);
            }

            // ---- Special parameters ----
            else if (paramType == typeof(CancellationToken))
                args[i] = cancel;
            else if (paramType == typeof(HttpListenerRequest))
                args[i] = request;
            else if (paramType == typeof(HttpListenerResponse))

                args[i] = response;
            else if (paramType == typeof(IPrincipal))
                args[i] = user;
            else
                args[i] = provider.GetService(paramType)
                    ?? throw new InvalidOperationException(
                        $"Unable to resolve service for type '{paramType.FullName}' " +
                        $"while invoking method '{method.DeclaringType?.FullName}.{method.Name}'. " +
                        "Ensure the service is registered in the dependency injection container."
                    );
        }

        object? result;

        try
        {
            result = method.Invoke(handler.Target, args);
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex; // cleaner catch
        }

        return await result.NormalizeToHttpResultAsync();
    }

    /// <summary>
    /// String value -> converts to target type (int, Guid, bool, etc.)
    /// </summary>
    private static object? ConvertTo(string? value, Type targetType)
    {
        if (value == null) return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        if (targetType == typeof(string)) return value;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        return Convert.ChangeType(value, underlying);
    }
}