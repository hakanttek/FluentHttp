using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace FluentHttp;

public static class DependencyInjection
{
    public static IServiceCollection AddFluentHttp(this IServiceCollection services)
    {
        services.AddSingleton<HttpServer>();
        services.AddSingleton<HttpListener>();
        return services;
    }
}
