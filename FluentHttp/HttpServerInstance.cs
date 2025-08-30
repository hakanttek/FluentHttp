using Microsoft.Extensions.DependencyInjection;

namespace FluentHttp;

public partial class HttpServer
{
    internal static IServiceCollection Services = new ServiceCollection();

    private static readonly Lazy<IServiceProvider> LazyProvider = new(() => Services.BuildServiceProvider());

    internal static IServiceProvider Provider => LazyProvider.Value;

    public static HttpServer Create(Action<IServiceCollection>? options)
    {
        Services.AddFluentHttp();
        return Provider.GetRequiredService<HttpServer>();
    }
}