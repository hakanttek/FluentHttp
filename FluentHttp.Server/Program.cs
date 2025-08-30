using FluentHttp;

await HttpServer.Create()
    .Get("/foo", async (request, response, user, cancel) =>
    {
        response.StatusCode = 200;
        await response.JsonAsync(new { message = "Hello, World!" }, cancel: cancel);
    })
    .Fallback(async (request, response, user, cancel) =>
    {
        response.StatusCode = 404;
        await response.JsonAsync(new { message = "Not found!" }, cancel: cancel);
    })
    .ListenOn(5000)
    .StartAsync();