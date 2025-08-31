using System.Net;
using System.Reflection;

namespace FluentHttp.Models;

public static class Extensions
{
    public static HttpResult Data(this HttpStatusCode statusCode, object data) => new((int)statusCode, data);

    internal static async Task<HttpResult> InvokeAsHttpResultAsync(this IServiceProvider provider, Delegate func)
    {
        var method = func.Method;
        var parameters = method.GetParameters();

        var args = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            args[i] = provider.GetService(parameters[i].ParameterType)
                ?? throw new InvalidOperationException(
                    $"Unable to resolve service for type '{parameters[i].ParameterType.FullName}' " +
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