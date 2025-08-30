using FluentHttp;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<HttpServer>();

var server = new HttpServer(logger).ListenOn(5000);
await server.StartAsync();