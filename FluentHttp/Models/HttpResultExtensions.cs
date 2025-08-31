using FluentHttp.Attributes;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace FluentHttp.Models;

public static class HttpResultExtensions
{
    public static HttpResult Data(this HttpStatusCode statusCode, object data) => new((int)statusCode, data);

    internal static async Task<HttpResult?> InvokeAsHttpResultAsync(
        this IServiceProvider provider,
        Delegate func,
        JsonSerializerOptions jSerializerOptions,
        Encoding bodyEncoding,
        HttpListenerRequest request,
        HttpListenerResponse response,
        IPrincipal? user,
        CancellationToken cancel = default)
    {
        var method = func.Method;
        var parameters = method.GetParameters();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var paramType = param.ParameterType;

            // ---- Attribute-based binding ----
            if (param.GetCustomAttribute<BodyAttribute>() is not null)
                args[i] = await request.JsonAsync(paramType, jSerializerOptions, bodyEncoding, cancel);
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
            else if(paramType == typeof(IPrincipal))
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
            result = method.Invoke(func.Target, args);
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException ?? ex; // cleaner catch
        }

        return await NormalizeToHttpResultAsync(result);
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

    private static async Task<HttpResult?> NormalizeToHttpResultAsync(object? output)
    {
        switch (output)
        {
            case HttpStatusCode code:
                return new HttpResult(code);

            case Task<HttpStatusCode> codeTask:
                return new HttpResult(await codeTask);

            case HttpResult result:
                return result;

            case Task<HttpResult> resultTask:
                return await resultTask;

            case Task task:
                await task;
                return null;

            default:
                throw new NotSupportedException(
                    $"The provided output type '{output?.GetType().FullName ?? "null"}' " +
                    "is not supported. Expected types are: " +
                    "HttpStatusCode, Task<HttpStatusCode>, HttpResult, Task<HttpResult>, or Task."
                );
        }
    }
}