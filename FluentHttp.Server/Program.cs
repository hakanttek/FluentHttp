using FluentHttp;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<HttpServer>();

var server = new HttpServer("http://localhost:5000/", logger);
await server.StartAsync();