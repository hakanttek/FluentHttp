using FluentHttp;
using FluentHttp.Models;
using System.Net;
using System.Security.Principal;

await HttpServer.Create()
    .Get("/foo", () =>
    {
        var res = new { message = "Hello, World!" };
        return HttpStatusCode.OK.Data(res);
    })
    .Fallback(() => HttpStatusCode.NotFound.Data(new { message = "Not found!" }))
    .ListenOn(5000)
    .StartAsync();