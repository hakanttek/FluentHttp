using FluentHttp;
using FluentHttp.Attributes;
using FluentHttp.Models;
using FluentHttp.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

await HttpServer.Create(options => options.AddScoped<FooService>())
.Get("/foo", () =>
{
    var res = new { message = "Hello, World!" };
    return HttpStatusCode.OK.Data(res);
})
.Get("/bar", ([Query] int baz) => HttpStatusCode.OK.Data(baz))
.Fallback(() => HttpStatusCode.NotFound.Data(new { message = "Not found!" }))
.ListenOn(5000)
.StartAsync();