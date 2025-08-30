using FluentHttp;

await HttpServer.Create()
    .Get("/foo", async (request, response, User) =>
    {
        response.StatusCode = 200;
        await response.JsonAsync(new { message = "Hello, World!" });
    })
    .Fallback(async (request, response, User) =>
    {
        response.StatusCode = 404;
        await response.JsonAsync(new { message = "Not found!" });
    })
    .ListenOn(5000)
    .StartAsync();