using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentHttp;

public partial class HttpServer
{
    internal static StaticConfiguration Services = new();

    private static readonly Lazy<IServiceProvider> LazyProvider = new(() => Services.BuildServiceProvider());

    internal static IServiceProvider Provider => LazyProvider.Value;

    public static HttpServer Create(Action<StaticConfiguration>? options = null)
    {
        options ??= opt => opt.AddLogger();
        options.Invoke(Services);
        Services.AddFluentHttp();
        return Provider.GetRequiredService<HttpServer>();
    }

    public class StaticConfiguration : ServiceCollection, IServiceCollection
    {
        private IServiceCollection Services => this;

        public void AddLogger(Action<ILoggingBuilder>? configure = null)
        {
            configure ??= builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            };
            Services.AddLogging(configure);
        }                
    }
}