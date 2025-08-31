using FluentHttp;
using FluentHttp.Attributes;
using FluentHttp.Models;
using FluentHttp.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

await HttpServer.Create(options => options.AddScoped<FooService>())
.Get("/foo", async (FooService service, [Query] int? bar, [Query] string? qux) =>
{
    var foos = await service.GetAsync(bar, qux);
    return foos.Any() 
        ? HttpStatusCode.OK.Data(foos)
        : HttpStatusCode.NotFound.Data("No foos found!");
})
.Fallback(() => HttpStatusCode.NotFound.Data(new { message = "Not found!" }))
.ListenOn(5000)
.StartAsync();