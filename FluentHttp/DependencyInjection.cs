using Microsoft.Extensions.DependencyInjection;

namespace FluentHttp;

public static class DependencyInjection
{
    public static IServiceCollection AddFluentHttp(this IServiceCollection services)
    {
        services.AddSingleton<HttpServer>();
        return services;
    }
}
