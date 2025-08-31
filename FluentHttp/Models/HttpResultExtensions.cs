using System.Net;
using System.Reflection;
using System.Security.Principal;

namespace FluentHttp.Models;

public static class HttpResultExtensions
{
    public static HttpResult Data(this HttpStatusCode statusCode, object data) => new((int)statusCode, data);

    internal static async Task<HttpResult?> InvokeAsHttpResultAsync(
        this IServiceProvider provider,
        Delegate func,
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

            if (paramType == typeof(CancellationToken))
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