using FluentHttp;
using FluentHttp.Attributes;
using FluentHttp.Models;
using FluentHttp.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

await HttpServer.Create(options =>
{
    options.AddScoped<FooService>();
    options.AddLogger();
})
.Get("/foo", async (FooService service, [Query] int? bar, [Query] string? qux) =>
{
    var foos = await service.GetAsync(bar, qux);
    return foos.Any() 
        ? HttpStatusCode.OK.Data(foos)
        : HttpStatusCode.NotFound.Data("No foos found!");
})
.Post("/foo", async (FooService service, [Body] Foo foo) =>
{
    await service.AddAsync(foo);
    return HttpStatusCode.Created;
})
.Fallback(() => HttpStatusCode.NotFound.Data(new { message = "Not found!" }))
.ListenOn(5000)
.StartAsync();