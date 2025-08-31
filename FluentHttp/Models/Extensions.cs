using System.Net;
using System.Reflection;

namespace FluentHttp.Models;

public static class Extensions
{
    public static HttpResult Data(this HttpStatusCode statusCode, object data) => new((int)statusCode, data);

    internal static async Task<HttpResult> InvokeAsHttpResultAsync(
        this IServiceProvider provider,
        Delegate func,
        HttpListenerRequest request,
        HttpListenerResponse response,
        CancellationToken cancel = default)
    {
        var method = func.Method;
        var parameters = method.GetParameters();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            var paramType = param.ParameterType;

            if (paramType == typeof(CancellationToken))
            {
                args[i] = cancel;
            }
            else if (paramType == typeof(HttpListenerRequest))
            {
                args[i] = request;
            }
            else if (paramType == typeof(HttpListenerResponse))
            {
                args[i] = response;
            }
            else
            {
                args[i] = provider.GetService(paramType)
                    ?? throw new InvalidOperationException(
                        $"Unable to resolve service for type '{paramType.FullName}' " +
                        $"while invoking method '{method.DeclaringType?.FullName}.{method.Name}'. " +
                        "Ensure the service is registered in the dependency injection container."
                    );
            }
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

    private static async Task<HttpResult> NormalizeToHttpResultAsync(object? output) =>
        output switch
        {
            HttpStatusCode code => new HttpResult(code),
            Task<HttpStatusCode> codeTask => new HttpResult(await codeTask),
            HttpResult result => result,
            Task<HttpResult> resultTask => await resultTask,
            _ => throw new NotSupportedException(
                $"The provided output type '{output?.GetType().FullName ?? "null"}' " +
                "is not supported. Expected types are: " +
                "HttpStatusCode, Task<HttpStatusCode>, HttpResult, or Task<HttpResult>."
            )
        };
}